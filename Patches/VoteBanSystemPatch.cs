﻿using HarmonyLib;

namespace TOZ.Patches
{
    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    class VoteBanSystemPatch
    {
        public static bool Prefix()
        {
            return !AmongUsClient.Instance.AmHost || !Options.DisableVoteBan.GetBool();
        }
    }
    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
    class VoteBanSystemPatchCmd
    {
        public static bool Prefix()
        {
            return !AmongUsClient.Instance.AmHost || !Options.DisableVoteBan.GetBool();
        }
    }
}
