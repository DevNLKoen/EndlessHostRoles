using System;
using System.Linq;
using AmongUs.Data;
using TOZ.AddOns.Common;
using TOZ.AddOns.Crewmate;
using TOZ.AddOns.Impostor;
using TOZ.Crewmate;
using TOZ.Impostor;
using TOZ.Neutral;
using HarmonyLib;

namespace TOZ.Patches;

class ExileControllerWrapUpPatch
{
    private static NetworkedPlayerInfo antiBlackout_LastExiled;

    public static NetworkedPlayerInfo AntiBlackout_LastExiled
    {
        get => antiBlackout_LastExiled;
        set => antiBlackout_LastExiled = value;
    }

    static void WrapUpPostfix(NetworkedPlayerInfo exiled)
    {
        if (AntiBlackout.OverrideExiledPlayer)
        {
            exiled = AntiBlackout_LastExiled;
        }

        bool DecidedWinner = false;
        if (!AmongUsClient.Instance.AmHost) return;
        AntiBlackout.RestoreIsDead(doSend: false);
        if (exiled != null)
        {
            if (!AntiBlackout.OverrideExiledPlayer && Main.ResetCamPlayerList.Contains(exiled.PlayerId))
                exiled.Object?.ResetPlayerCam(1f);

            exiled.IsDead = true;
            Main.PlayerStates[exiled.PlayerId].deathReason = PlayerState.DeathReason.Vote;
            var role = exiled.GetCustomRole();

            if (role.Is(Team.Impostor))
            {
                Damocles.OnImpostorEjected();
            }
            else
            {
                Damocles.OnCrewmateEjected();
            }

            switch (role)
            {
                case CustomRoles.Jester:
                    if (DecidedWinner) CustomWinnerHolder.ShiftWinnerAndSetWinner(CustomWinner.Jester);
                    else CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Jester);
                    CustomWinnerHolder.WinnerIds.Add(exiled.PlayerId);
                    DecidedWinner = true;
                    break;
                case CustomRoles.Medic:
                    Medic.IsDead(exiled.Object);
                    break;
            }

            if (Executioner.CheckExileTarget(exiled)) DecidedWinner = true;
            if (Lawyer.CheckExileTarget(exiled /*, DecidedWinner*/)) DecidedWinner = false;

            Main.PlayerStates[exiled.PlayerId].SetDead();
        }

        if (AmongUsClient.Instance.AmHost && Main.IsFixedCooldown)
            Main.RefixCooldownDelay = Options.DefaultKillCooldown - 3f;

        NiceSwapper.OnExileFinish();

        foreach (PlayerControl pc in Main.AllPlayerControls)
        {
            pc.ResetKillCooldown();
            pc.RpcResetAbilityCooldown();
            PetsPatch.RpcRemovePet(pc);
        }

        if (Options.RandomSpawn.GetBool() || Options.CurrentGameMode != CustomGameMode.Standard)
        {
            RandomSpawn.SpawnMap map = Main.NormalOptions.MapId switch
            {
                0 => new RandomSpawn.SkeldSpawnMap(),
                1 => new RandomSpawn.MiraHQSpawnMap(),
                2 => new RandomSpawn.PolusSpawnMap(),
                3 => new RandomSpawn.DleksSpawnMap(),
                5 => new RandomSpawn.FungleSpawnMap(),
                _ => null
            };
            if (map != null) Main.AllAlivePlayerControls.Do(map.RandomTeleport);
        }

        FallFromLadder.Reset();
        Utils.CountAlivePlayers(true);
        Utils.AfterMeetingTasks();
        Utils.SyncAllSettings();
        Utils.NotifyRoles(ForceLoop: true);
    }

    static void WrapUpFinalizer(NetworkedPlayerInfo exiled)
    {
        // Even if an exception occurs in WrapUpPostfix, this part will be executed reliably.
        if (AmongUsClient.Instance.AmHost)
        {
            LateTask.New(() =>
            {
                exiled = AntiBlackout_LastExiled;
                AntiBlackout.SendGameData();
                if (AntiBlackout.OverrideExiledPlayer && // State where the exile target is overwritten (no need to execute if it is not overwritten)
                    exiled != null &&
                    exiled.Object != null)
                {
                    exiled.Object.RpcExileV2();
                }
            }, 0.8f, "Restore IsDead Task");
            LateTask.New(() =>
            {
                Main.AfterMeetingDeathPlayers.Do(x =>
                {
                    var player = Utils.GetPlayerById(x.Key);
                    var state = Main.PlayerStates[x.Key];
                    Logger.Info($"{player.GetNameWithRole().RemoveHtmlTags()} died with {x.Value}", "AfterMeetingDeath");
                    state.deathReason = x.Value;
                    state.SetDead();
                    player?.RpcExileV2();
                    if (x.Value == PlayerState.DeathReason.Suicide)
                        player?.SetRealKiller(player, true);
                    if (Main.ResetCamPlayerList.Contains(x.Key))
                        player?.ResetPlayerCam(1f);
                    Utils.AfterPlayerDeathTasks(player);
                });
                Main.AfterMeetingDeathPlayers.Clear();
            }, 0.9f, "AfterMeetingDeathPlayers Task");
        }

        GameStates.AlreadyDied |= !Utils.IsAllAlive;
        RemoveDisableDevicesPatch.UpdateDisableDevices();
        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
        Logger.Info("Start task phase", "Phase");

        if (Lovers.IsChatActivated && Lovers.PrivateChat.GetBool()) return;

        bool showRemainingKillers = Options.EnableKillerLeftCommand.GetBool() && Options.ShowImpRemainOnEject.GetBool();
        bool appendEjectionNotify = CheckForEndVotingPatch.EjectionText != string.Empty;
        Logger.Msg($"Ejection Text: {CheckForEndVotingPatch.EjectionText}", "ExilePatch");
        if ((showRemainingKillers || appendEjectionNotify) && Options.CurrentGameMode == CustomGameMode.Standard)
        {
            LateTask.New(() =>
            {
                var text = showRemainingKillers ? Utils.GetRemainingKillers(notify: true) : string.Empty;
                text = $"<#ffffff>{text}</color>";
                var r = IRandom.Instance;
                foreach (var pc in Main.AllAlivePlayerControls)
                {
                    string finalText = text;
                    if (NameNotifyManager.Notice.TryGetValue(pc.PlayerId, out var notify))
                    {
                        finalText = $"\n{notify.TEXT}\n{finalText}";
                    }

                    if (appendEjectionNotify && !finalText.Contains(CheckForEndVotingPatch.EjectionText, StringComparison.OrdinalIgnoreCase))
                    {
                        finalText = $"\n<#ffffff>{CheckForEndVotingPatch.EjectionText}</color>\n{finalText}";
                    }

                    if (!showRemainingKillers) finalText = finalText.TrimStart();

                    pc.Notify(finalText, r.Next(7, 13));
                }
            }, 0.5f, log: false);
        }

        LateTask.New(() => ChatManager.SendPreviousMessagesToAll(clear: true), 3f, log: false);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            try
            {
                WrapUpPostfix(__instance.exiled);
            }
            finally
            {
                WrapUpFinalizer(__instance.exiled);
            }
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            try
            {
                WrapUpPostfix(__instance.exiled);
            }
            finally
            {
                WrapUpFinalizer(__instance.exiled);
            }
        }
    }
}

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
class PolusExileHatFixPatch
{
    public static void Prefix(PbExileController __instance)
    {
        __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
    }
}