using System;
using AmongUs.Data;
using Discord;
using HarmonyLib;

namespace TOZ.Patches
{
    // Originally from Town of Us Rewritten, by Det
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public class DiscordRPC
    {
        private static string lobbycode = "";
        private static string region = "";

        public static void Prefix([HarmonyArgument(0)] Activity activity)
        {
            if (activity == null) return;

            var details = $"TOZ v{Main.PluginDisplayVersion}";
            activity.Details = details;

            try
            {
                if (activity.State != "In Menus")
                {
                    if (!DataManager.Settings.Gameplay.StreamerMode)
                    {
                        if (GameStates.IsLobby)
                        {
                            lobbycode = GameStartManager.Instance.GameRoomNameCode.text;
                            region = Utils.GetRegionName();
                        }

                        if (lobbycode != "" && region != "")
                        {
                            details = $"TOZ - {lobbycode} ({region})";
                        }

                        activity.Details = details;
                    }
                    else
                    {
                        details = $"TOZ v{Main.PluginDisplayVersion}";

                        activity.Details = details;
                    }
                }
            }

            catch (ArgumentException ex)
            {
                Logger.Error("Error in updating discord rpc", "DiscordPatch");
                Logger.Exception(ex, "DiscordPatch");
                details = $"TOZ v{Main.PluginDisplayVersion}";
                activity.Details = details;
            }
        }
    }
}