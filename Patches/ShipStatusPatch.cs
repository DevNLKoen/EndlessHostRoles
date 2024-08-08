using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx;
using TOZ.AddOns.Crewmate;
using TOZ.AddOns.Impostor;
using TOZ.Crewmate;
using TOZ.Modules;
using TOZ.Neutral;
using TOZ.Patches;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace TOZ;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
class ShipFixedUpdatePatch
{
    public static void Postfix( /*ShipStatus __instance*/)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Main.IsFixedCooldown && Main.RefixCooldownDelay >= 0)
        {
            Main.RefixCooldownDelay -= Time.fixedDeltaTime;
        }
        else if (!float.IsNaN(Main.RefixCooldownDelay))
        {
            Utils.MarkEveryoneDirtySettingsV4();
            Main.RefixCooldownDelay = float.NaN;
            Logger.Info("Refix Cooldown", "CoolDown");
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
public static class MessageReaderUpdateSystemPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        try
        {
            if (systemType is SystemTypes.Ventilation or SystemTypes.Security or SystemTypes.Decontamination or SystemTypes.Decontamination2 or SystemTypes.Decontamination3) return true;

            var amount = MessageReader.Get(reader).ReadByte();
            if (EAC.CheckInvalidSabotage(systemType, player, amount))
            {
                Logger.Info("EAC patched Sabotage RPC", "MessageReaderUpdateSystemPatch");
                return false;
            }

            return RepairSystemPatch.Prefix(__instance, systemType, player, amount);
        }
        catch
        {
        }

        return true;
    }

    public static void Postfix( /*ShipStatus __instance,*/ [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        try
        {
            if (systemType is SystemTypes.Ventilation or SystemTypes.Security or SystemTypes.Decontamination or SystemTypes.Decontamination2 or SystemTypes.Decontamination3) return;
            RepairSystemPatch.Postfix( /*__instance,*/ systemType, player, MessageReader.Get(reader).ReadByte());
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(byte))]
class RepairSystemPatch
{
    public static bool Prefix(ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] byte amount)
    {
        Logger.Msg($"SystemType: {systemType}, PlayerName: {player.GetNameWithRole().RemoveHtmlTags()}, amount: {amount}", "RepairSystem");
        if (RepairSender.Enabled && AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
            Logger.SendInGame($"SystemType: {systemType}, PlayerName: {player.GetNameWithRole().RemoveHtmlTags()}, amount: {amount}");

        if (!AmongUsClient.Instance.AmHost) return true; // Execute the following only on the host

        if ((Options.CurrentGameMode != CustomGameMode.Standard || Options.DisableSabotage.GetBool()) && systemType == SystemTypes.Sabotage) return false;

        // Note: "SystemTypes.Laboratory" сauses bugs in the Host, it is better not to use
        if (player.Is(CustomRoles.Fool) && (systemType is SystemTypes.Comms or SystemTypes.Electrical))
        {
            return false;
        }

        switch (player.GetCustomRole())
        {
            case CustomRoles.SabotageMaster:
                SabotageMaster.RepairSystem(player.PlayerId, __instance, systemType, amount);
                Utils.NotifyRoles(SpecifySeer: player, SpecifyTarget: player);
                break;
        }

        switch (systemType)
        {
            case SystemTypes.Electrical when amount <= 4 && Main.NormalOptions.MapId == 4:
                if (Options.DisableAirshipViewingDeckLightsPanel.GetBool() && Vector2.Distance(player.transform.position, new(-12.93f, -11.28f)) <= 2f) return false;
                if (Options.DisableAirshipGapRoomLightsPanel.GetBool() && Vector2.Distance(player.transform.position, new(13.92f, 6.43f)) <= 2f) return false;
                if (Options.DisableAirshipCargoLightsPanel.GetBool() && Vector2.Distance(player.transform.position, new(30.56f, 2.12f)) <= 2f) return false;
                break;
            case SystemTypes.Sabotage when AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay:
                if (Options.CurrentGameMode != CustomGameMode.Standard) return false;
                if (player.IsRoleBlocked())
                {
                    player.Notify(BlockedAction.Sabotage.GetBlockNotify());
                    return false;
                }

                if (player.Is(Team.Impostor) && !player.IsAlive() && Options.DeadImpCantSabotage.GetBool()) return false;
                return player.GetCustomRole() switch
                {
                    CustomRoles.Traitor when Traitor.CanSabotage.GetBool() => true,
                    _ => Main.PlayerStates[player.PlayerId].Role.CanUseSabotage(player)
                };
            case SystemTypes.Security when amount == 1:
                var camerasDisabled = (MapNames)Main.NormalOptions.MapId switch
                {
                    MapNames.Skeld => Options.DisableSkeldCamera.GetBool(),
                    MapNames.Polus => Options.DisablePolusCamera.GetBool(),
                    MapNames.Airship => Options.DisableAirshipCamera.GetBool(),
                    _ => false
                };
                if (camerasDisabled)
                {
                    player.Notify(Translator.GetString("CamerasDisabledNotify"), 15f);
                }

                return !camerasDisabled;
        }

        return true;
    }

    public static void Postfix( /*ShipStatus __instance,*/
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] byte amount)
    {
        Camouflage.CheckCamouflage();

        switch (systemType)
        {
            case SystemTypes.Electrical when amount <= 4:
                var SwitchSystem = ShipStatus.Instance?.Systems?[SystemTypes.Electrical]?.Cast<SwitchSystem>();
                if (SwitchSystem is { IsActive: true })
                {
                    switch (Main.PlayerStates[player.PlayerId].Role)
                    {
                        case SabotageMaster:
                            Logger.Info($"{player.GetNameWithRole().RemoveHtmlTags()} instant-fix-lights", "SwitchSystem");
                            SabotageMaster.SwitchSystemRepair(player.PlayerId, SwitchSystem, amount);
                            Utils.NotifyRoles(SpecifySeer: player, SpecifyTarget: player);
                            break;
                    }

                    if (player.Is(CustomRoles.Damocles) && Damocles.countRepairSabotage) Damocles.OnRepairSabotage(player.PlayerId);
                }

                break;
            case SystemTypes.Reactor:
            case SystemTypes.LifeSupp:
            case SystemTypes.Comms:
            case SystemTypes.Laboratory:
            case SystemTypes.HeliSabotage:
            case SystemTypes.Electrical:
                if (player.Is(CustomRoles.Damocles) && Damocles.countRepairSabotage) Damocles.OnRepairSabotage(player.PlayerId);
                break;
        }
    }

    public static void CheckAndOpenDoorsRange(ShipStatus __instance, int amount, int min, int max)
    {
        var Ids = new List<int>();
        for (var i = min; i <= max; i++)
        {
            Ids.Add(i);
        }

        CheckAndOpenDoors(__instance, amount, [.. Ids]);
    }

    private static void CheckAndOpenDoors(ShipStatus __instance, int amount, params int[] DoorIds)
    {
        if (!DoorIds.Contains(amount)) return;
        foreach (int id in DoorIds)
        {
            __instance.RpcUpdateSystem(SystemTypes.Doors, (byte)id);
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
class CloseDoorsPatch
{
    public static bool Prefix( /*ShipStatus __instance, */ [HarmonyArgument(0)] SystemTypes room)
    {
        bool allow = !Options.DisableSabotage.GetBool() && Options.CurrentGameMode is not CustomGameMode.SoloKombat and not CustomGameMode.FFA and not CustomGameMode.MoveAndStop and not CustomGameMode.HotPotato;

        if (Options.DisableCloseDoor.GetBool()) allow = false;

        Logger.Info($"({room}) => {(allow ? "Allowed" : "Blocked")}", "DoorClose");
        return allow;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
class StartPatch
{
    public static void Postfix()
    {
        Logger.CurrentMethod();
        Logger.Info("-----------Game start-----------", "Phase");

        Utils.CountAlivePlayers(true);

        if (Options.AllowConsole.GetBool())
        {
            if (!ConsoleManager.ConsoleActive && ConsoleManager.ConsoleEnabled)
                ConsoleManager.CreateConsole();
        }
        else
        {
            if (ConsoleManager.ConsoleActive && !DebugModeManager.AmDebugger)
            {
                ConsoleManager.DetachConsole();
                Logger.SendInGame("Sorry, console use is prohibited in this room, so your console has been turned off");
            }
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
class StartMeetingPatch
{
    public static void Prefix( /*ShipStatus __instance, PlayerControl reporter,*/ NetworkedPlayerInfo target)
    {
        MeetingStates.ReportTarget = target;
        MeetingStates.DeadBodies = Object.FindObjectsOfType<DeadBody>();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
class BeginPatch
{
    public static void Postfix()
    {
        Logger.CurrentMethod();

        //Should I initialize the host role here? - no
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (Options.DisableTaskWin.GetBool() || Options.NoGameEnd.GetBool() || TaskState.InitialTotalTasks == 0 || (Options.DisableTaskWinIfAllCrewsAreDead.GetBool() && !Main.AllAlivePlayerControls.Any(x => x.Is(CustomRoleTypes.Crewmate))) || (Options.DisableTaskWinIfAllCrewsAreConverted.GetBool() && Main.AllPlayerControls.Where(x => x.Is(Team.Crewmate) && x.GetRoleTypes() is RoleTypes.Crewmate or RoleTypes.Engineer or RoleTypes.Scientist or RoleTypes.CrewmateGhost or RoleTypes.GuardianAngel).All(x => x.GetCustomSubRoles().Any(y => y.IsConverted()))) || Options.CurrentGameMode != CustomGameMode.Standard)
        {
            __result = false;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigameSetFilterTextPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget != null && Options.GhostCanSeeOtherRoles.GetBool())
        {
            var id = __instance.HauntTarget.PlayerId;
            __instance.FilterText.text = Utils.GetDisplayRoleName(id) + Utils.GetProgressText(id);
            return false;
        }

        return true;
    }
}