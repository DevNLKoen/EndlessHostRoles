using System.Linq;
using TOZ.AddOns.Common;
using TOZ.Crewmate;
using TOZ.Impostor;
using TOZ.Modules;
using TOZ.Neutral;
using Hazel;

namespace TOZ;

public static class NameColorManager
{
    public static string ApplyNameColorData(this string name, PlayerControl seer, PlayerControl target, bool isMeeting)
    {
        if (!AmongUsClient.Instance.IsGameStarted) return name;

        if (!TryGetData(seer, target, out var colorCode))
        {
            if (KnowTargetRoleColor(seer, target, isMeeting, out var color))
                colorCode = color == "" ? target.GetRoleColorCode() : color;
        }

        string openTag = "", closeTag = "";
        if (colorCode != "")
        {
            if (!colorCode.StartsWith('#'))
                colorCode = "#" + colorCode;
            openTag = $"<{colorCode}>";
            closeTag = "</color>";
        }

        return openTag + name + closeTag;
    }

    private static bool KnowTargetRoleColor(PlayerControl seer, PlayerControl target, bool isMeeting, out string color)
    {
        color = "";

        switch (Options.CurrentGameMode)
        {
            case CustomGameMode.FFA when FFAManager.FFATeamMode.GetBool():
                color = FFAManager.TeamColors[FFAManager.PlayerTeams[target.PlayerId]];
                return true;
            case CustomGameMode.MoveAndStop:
                color = "#ffffff";
                return true;
            case CustomGameMode.HotPotato:
                (byte HolderID, byte LastHolderID, _, _) = HotPotatoManager.GetState();
                if (target.PlayerId == HolderID) color = "#000000";
                else if (target.PlayerId == LastHolderID) color = "#00ffff";
                else color = "#ffffff";
                return true;
            case CustomGameMode.HideAndSeek:
                return HnSManager.KnowTargetRoleColor(seer, target, ref color);
        }


        // Custom Teams
        if (CustomTeamManager.AreInSameCustomTeam(seer.PlayerId, target.PlayerId) && CustomTeamManager.IsSettingEnabledForPlayerTeam(seer.PlayerId, CTAOption.KnowRoles)) color = Main.RoleColors[target.GetCustomRole()];

        // Add-ons
        if (target.Is(CustomRoles.Glow) && Utils.IsActive(SystemTypes.Electrical)) color = Main.RoleColors[CustomRoles.Glow];
        if (seer.Is(CustomRoles.Contagious) && target.Is(CustomRoles.Contagious) && Virus.TargetKnowOtherTarget.GetBool()) color = Main.RoleColors[CustomRoles.Virus];


        var seerRole = seer.GetCustomRole();
        var targetRole = target.GetCustomRole();

        // If 2 players have the same role and that role is a NK role, they can see each other's name color
        if (seerRole.IsNK() && seerRole == targetRole)
        {
            color = Main.RoleColors[seerRole];
        }

        // Check if the seer can see the target's role color
        color = seerRole switch
        {
            CustomRoles.Executioner when Executioner.Target.TryGetValue(seer.PlayerId, out var exeTarget) && exeTarget == target.PlayerId => "000000",
            CustomRoles.Virus when target.Is(CustomRoles.Contagious) => Main.RoleColors[CustomRoles.Contagious],
            CustomRoles.Pyromaniac when ((Pyromaniac)Main.PlayerStates[seer.PlayerId].Role).DousedList.Contains(target.PlayerId) => "#BA4A00",
            CustomRoles.Glitch when target.IsRoleBlocked() => Main.RoleColors[seerRole],
            _ => color
        };

        // Check if the role color can be seen based on the target's role
        color = targetRole switch
        {
            CustomRoles.Virus when seer.Is(CustomRoles.Contagious) => Main.RoleColors[CustomRoles.Virus],
            _ => color
        };

        // Visionary and Necroview
        if (((seer.Is(CustomRoles.Necroview) && target.Data.IsDead && !target.IsAlive())))
        {
            color = target.GetCustomRoleTypes() switch
            {
                CustomRoleTypes.Impostor => Main.ImpostorColor,
                CustomRoleTypes.Crewmate => Main.CrewmateColor,
                CustomRoleTypes.Neutral => Main.NeutralColor,
                _ => color
            };

            if (target.Is(CustomRoles.Rascal)) color = Main.ImpostorColor;

            if (target.Is(CustomRoles.Contagious)) color = Main.NeutralColor;
            if (target.Is(CustomRoles.Recruit)) color = Main.NeutralColor;
        }

        // If the color was determined, return true, else, check if the seer can see the target's role color without knowing the color
        if (color != "") return true;
        return seer == target
               || (Main.GodMode.Value && seer.AmOwner)
               || (Options.CurrentGameMode is CustomGameMode.FFA or CustomGameMode.MoveAndStop)
               || (Main.PlayerStates[seer.Data.PlayerId].IsDead && seer.Data.IsDead && !seer.IsAlive() && Options.GhostCanSeeOtherRoles.GetBool())
               || (seer.Is(CustomRoles.Mimic) && Main.PlayerStates[target.Data.PlayerId].IsDead && target.Data.IsDead && !target.IsAlive() && Options.MimicCanSeeDeadRoles.GetBool())
               || target.Is(CustomRoles.GM)
               || seer.Is(CustomRoles.GM)
               || seer.Is(CustomRoles.God)
               || (seer.Is(CustomRoleTypes.Impostor) && target.Is(CustomRoleTypes.Impostor))
               || (seer.Is(CustomRoles.Traitor) && target.Is(Team.Impostor))
               || (seer.Is(CustomRoles.Sidekick) && target.Is(CustomRoles.Sidekick))
               || (target.Is(CustomRoles.SuperStar) && Options.EveryOneKnowSuperStar.GetBool())
               || (target.Is(CustomRoles.Doctor) && !target.HasEvilAddon() && Options.DoctorVisibleToEveryone.GetBool())
               || (target.Is(CustomRoles.Gravestone) && Main.PlayerStates[target.Data.PlayerId].IsDead)
               || (target.Is(CustomRoles.Mayor) && Mayor.MayorRevealWhenDoneTasks.GetBool() && target.GetTaskState().IsTaskFinished)
               || Main.PlayerStates.Values.Any(x => x.Role.KnowRole(seer, target));
    }

    public static bool TryGetData(PlayerControl seer, PlayerControl target, out string colorCode)
    {
        colorCode = "";
        var state = Main.PlayerStates[seer.PlayerId];
        if (!state.TargetColorData.TryGetValue(target.PlayerId, out var value)) return false;
        colorCode = value;
        return true;
    }

    public static void Add(byte seerId, byte targetId, string colorCode = "")
    {
        if (colorCode == "")
        {
            var target = Utils.GetPlayerById(targetId);
            if (target == null) return;
            colorCode = target.GetRoleColorCode();
        }

        var state = Main.PlayerStates[seerId];
        if (state.TargetColorData.TryGetValue(targetId, out var value) && colorCode == value) return;
        state.TargetColorData.Add(targetId, colorCode);

        SendRPC(seerId, targetId, colorCode);
    }

    public static void Remove(byte seerId, byte targetId)
    {
        var state = Main.PlayerStates[seerId];
        if (!state.TargetColorData.ContainsKey(targetId)) return;
        state.TargetColorData.Remove(targetId);

        SendRPC(seerId, targetId);
    }

    public static void RemoveAll(byte seerId)
    {
        Main.PlayerStates[seerId].TargetColorData.Clear();

        SendRPC(seerId);
    }

    private static void SendRPC(byte seerId, byte targetId = byte.MaxValue, string colorCode = "")
    {
        if (!AmongUsClient.Instance.AmHost) return;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetNameColorData, SendOption.Reliable);
        writer.Write(seerId);
        writer.Write(targetId);
        writer.Write(colorCode);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void ReceiveRPC(MessageReader reader)
    {
        byte seerId = reader.ReadByte();
        byte targetId = reader.ReadByte();
        string colorCode = reader.ReadString();

        if (targetId == byte.MaxValue)
            RemoveAll(seerId);
        else if (colorCode == "")
            Remove(seerId, targetId);
        else
            Add(seerId, targetId, colorCode);
    }
}