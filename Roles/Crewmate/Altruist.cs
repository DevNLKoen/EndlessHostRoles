﻿using AmongUs.GameOptions;

namespace TOZ.Crewmate
{
    public static class Altruist
    {
        public static void SetupCustomOption() => Options.SetupRoleOptions(642600, TabGroup.CrewmateRoles, CustomRoles.Altruist);

        public static void OnKilled(PlayerControl killer)
        {
            if (killer == null) return;
            if (!killer.GetCustomRole().IsImpostor()) return;

            killer.RpcSetCustomRole(killer.Is(RoleTypes.Shapeshifter) ? CustomRoles.ShapeshifterTOZ : CustomRoles.ImpostorTOZ);
        }
    }
}