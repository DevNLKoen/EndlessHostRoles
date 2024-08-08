using AmongUs.GameOptions;
using Hazel;
using TOZ.Modules;
using static TOZ.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TOZ.Crewmate;
using TOZ.Neutral;
using static TOZ.Translator;
using TOZ.Impostor;

namespace TOZ.Crewmate;

public class Chameleon : RoleBase
{
    private const int Id = 6300;
    private static List<byte> PlayerIdList = [];

    public static OptionItem ChameleonCooldown;
    public static OptionItem ChameleonDuration;
    public static OptionItem ChameleonUseLimitOpt;
    public static OptionItem ChameleonAbilityUseGainWithEachTaskCompleted;
    public static OptionItem AbilityChargesWhenFinishedTasks;


    private int CD;

    private float Cooldown;
    private float Duration;

    private long InvisTime;

    private long lastFixedTime;
    private long lastTime;

    private byte ChameleonId;

    private int ventedId;
    private bool VentNormallyOnCooldown;

    public override bool IsEnable => PlayerIdList.Count > 0;

    bool CanGoInvis => GameStates.IsInTask && InvisTime == -10 && lastTime == -10;
    bool IsInvis => InvisTime != -10;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Chameleon);
        ChameleonCooldown = new FloatOptionItem(Id + 2, "ChameleonCooldown", new(1f, 60f, 1f), 20f, TabGroup.CrewmateRoles).SetParent(CustomRoleSpawnChances[CustomRoles.Chameleon])
            .SetValueFormat(OptionFormat.Seconds);
        ChameleonDuration = new FloatOptionItem(Id + 3, "ChameleonDuration", new(1f, 30f, 1f), 10f, TabGroup.CrewmateRoles).SetParent(CustomRoleSpawnChances[CustomRoles.Chameleon])
            .SetValueFormat(OptionFormat.Seconds);
        ChameleonUseLimitOpt = new IntegerOptionItem(Id + 4, "AbilityUseLimit", new(0, 20, 1), 1, TabGroup.CrewmateRoles).SetParent(CustomRoleSpawnChances[CustomRoles.Chameleon])
            .SetValueFormat(OptionFormat.Times);
        ChameleonAbilityUseGainWithEachTaskCompleted = new FloatOptionItem(Id + 5, "AbilityUseGainWithEachTaskCompleted", new(0f, 5f, 0.05f), 0.5f, TabGroup.CrewmateRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Chameleon])
            .SetValueFormat(OptionFormat.Times);
        AbilityChargesWhenFinishedTasks = new FloatOptionItem(Id + 6, "AbilityChargesWhenFinishedTasks", new(0f, 5f, 0.05f), 0.2f, TabGroup.CrewmateRoles)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Chameleon])
            .SetValueFormat(OptionFormat.Times);
    }

    public override void Init()
    {
        PlayerIdList = [];
        InvisTime = -10;
        lastTime = -10;
        ventedId = -10;
        CD = 0;
        ChameleonId = byte.MaxValue;
    }

    public override void Add(byte playerId)
    {
        PlayerIdList.Add(playerId);
        ChameleonId = playerId;

        InvisTime = -10;
        lastTime = -10;
        ventedId = -10;
        CD = 0;

        playerId.SetAbilityUseLimit(ChameleonUseLimitOpt.GetInt());
        Cooldown = Chameleon.ChameleonCooldown.GetFloat();
        Duration = Chameleon.ChameleonDuration.GetFloat();
        VentNormallyOnCooldown = true;

        Main.ResetCamPlayerList.Add(playerId);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {

        AURoleOptions.EngineerCooldown = Cooldown;
        AURoleOptions.EngineerInVentMaxTime = Duration;
    }

    public override bool CanUseImpostorVentButton(PlayerControl pc)
    {
        return pc.Data.RoleType != RoleTypes.Engineer;
    }

    void SendRPC()
    {
        if (!IsEnable || !Utils.DoRPC) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetChameleonTimer, SendOption.Reliable);
        writer.Write(ChameleonId);
        writer.Write(InvisTime.ToString());
        writer.Write(lastTime.ToString());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public void ReceiveRPC(MessageReader reader)
    {
        InvisTime = long.Parse(reader.ReadString());
        lastTime = long.Parse(reader.ReadString());
    }

    public override void AfterMeetingTasks()
    {
        InvisTime = -10;
        lastTime = Utils.TimeStamp;
        SendRPC();
    }

    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!GameStates.IsInTask || !IsEnable || player == null) return;

        var now = Utils.TimeStamp;

        if (lastTime != -10)
        {
            if (!player.IsModClient())
            {
                var cooldown = lastTime + (long)Cooldown - now;
                if ((int)cooldown != CD) player.Notify(string.Format(GetString("CDPT"), cooldown + 1), 1.1f);
                CD = (int)cooldown;
            }

            if (lastTime + (long)Cooldown < now)
            {
                lastTime = -10;
                if (!player.IsModClient()) player.Notify(GetString("SwooperCanVent"), 300f);
                SendRPC();
                CD = 0;
            }
        }

        if (lastFixedTime != now && InvisTime != -10)
        {
            lastFixedTime = now;
            bool refresh = false;
            var remainTime = InvisTime + (long)Duration - now;
            switch (remainTime)
            {
                case < 0:
                    lastTime = now;
                    var pos = player.Pos();
                    player.MyPhysics?.RpcBootFromVent(ventedId == -10 ? Main.LastEnteredVent[player.PlayerId].Id : ventedId);
                    player.Notify(GetString("SwooperInvisStateOut"));
                    InvisTime = -10;
                    SendRPC();
                    refresh = true;
                    LateTask.New(() => { player.TP(pos); }, 0.5f, log: false);
                    break;
                case <= 10 when !player.IsModClient():
                    player.Notify(string.Format(GetString("SwooperInvisStateCountdown"), remainTime + 1));
                    break;
            }

            if (refresh) SendRPC();
        }
    }

    public override void OnCoEnterVent(PlayerPhysics __instance, int ventId)
    {
        if (!AmongUsClient.Instance.AmHost || IsInvis) return;

        var pc = __instance.myPlayer;
        LateTask.New(() =>
        {
            float limit = pc.GetAbilityUseLimit();
            if (CanGoInvis && (limit >= 1))
            {
                ventedId = ventId;

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, SendOption.Reliable, pc.GetClientId());
                writer.WritePacked(ventId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                InvisTime = Utils.TimeStamp;
                SendRPC();
                pc.Notify(GetString("SwooperInvisState"), Duration);
            }
            else if (!VentNormallyOnCooldown)
            {
                __instance.RpcBootFromVent(ventId);
                pc.Notify(GetString("SwooperInvisInCooldown"));
            }
        }, 0.5f, "Swooper Vent");
    }

    public override void OnEnterVent(PlayerControl pc, Vent vent)
    {
        if (!IsInvis || InvisTime == Utils.TimeStamp) return;

        InvisTime = -10;
        lastTime = Utils.TimeStamp;
        SendRPC();

        pc?.MyPhysics?.RpcBootFromVent(vent.Id);
        pc.Notify(GetString("SwooperInvisStateOut"));
    }

    public override string GetSuffix(PlayerControl pc, PlayerControl _, bool hud = false, bool m = false)
    {
        if (!hud || pc == null || !GameStates.IsInTask || !PlayerControl.LocalPlayer.IsAlive()) return string.Empty;
        if (Main.PlayerStates[pc.PlayerId].Role is not Chameleon sw) return string.Empty;

        var str = new StringBuilder();
        if (sw.IsInvis)
        {
            var remainTime = sw.InvisTime + (long)sw.Duration - Utils.TimeStamp;
            str.Append(string.Format(GetString("SwooperInvisStateCountdown"), remainTime + 1));
        }
        else if (sw.lastTime != -10)
        {
            var cooldown = sw.lastTime + (long)sw.Cooldown - Utils.TimeStamp;
            str.Append(string.Format(GetString("SwooperInvisCooldownRemain"), cooldown + 1));
        }
        else
        {
            str.Append(GetString("SwooperCanVent"));
        }

        return str.ToString();
    }

    public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (Medic.ProtectList.Contains(target.PlayerId)) return false;
        if (target.Is(CustomRoles.Bait)) return true;

        if (!IsInvis) return true;
        killer.SetKillCooldown();
        target.SetRealKiller(killer);
        target.RpcCheckAndMurder(target);
        return false;
    }
}