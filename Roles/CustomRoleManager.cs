using AmongUs.GameOptions;
using System;
using System.Text;
using static TOZ.Modules.CustomTeamManager;
using static UnityEngine.ParticleSystem;
using TOZ.AddOns.Common;
using TOZ.AddOns.Impostor;
using TOZ.Crewmate;
using TOZ.Impostor;
using TOZ.Neutral;
using TOZ;
using System.Collections.Generic;
using System.Linq;

namespace TOZ.Roles.Core;

public static class CustomRoleManager
{
    public static readonly Dictionary<CustomRoles, RoleBase> RoleClass = [];
    public static List<RoleBase> AllEnabledRoles => Main.PlayerStates.Values.Select(x => x.Role).ToList(); //Since there are classes which use object attributes and playerstate is not removed.

    public static bool OtherCollectionsSet = false;
    public static List<RoleBase> GetNormalOptions(RoleOptionType type)
    {
        List<RoleBase> roles = [];
        foreach (var role in RoleClass.Values)
        {
            //if (role.IsExperimental) continue;

            if (role.GetType() == type.GetType())
            {
                roles.Add(role);
            }
        }
        return roles;
    }
    public static List<RoleBase> GetExperimentalOptions(Team team)
    {
        List<RoleBase> roles = [];
        switch (team)
        {
            case Team.Crewmate:
                roles = RoleClass.Where(r => r.Value.IsExperimental && r.Key.IsCrewmate()).Select(r => r.Value).ToList();
                break;

            case Team.Impostor:
                roles = RoleClass.Where(r => r.Value.IsExperimental && r.Key.IsImpostorTeam()).Select(r => r.Value).ToList();
                break;

            case Team.Neutral:
                roles = RoleClass.Where(r => r.Value.IsExperimental && r.Key.IsNeutralTeamV2()).Select(r => r.Value).ToList();
                break;

            default:
                Logger.Info("Unsupported team was sent.", "GetExperimentalOptions");
                break;
        }
        return roles;
    }
    //public static bool IsOptBlackListed(this Type role) => CustomRolesHelper.DuplicatedRoles.ContainsValue(role);


    /// <summary>
    /// If the role protect others players
    /// </summary>
    public static bool OnCheckMurderAsTargetOnOthers(PlayerControl killer, PlayerControl target)
    {
        // return true when need to cancel the kill target
        // "Any()" defines a function that returns true, and converts to false to cancel the kill
        return !AllEnabledRoles.Any(RoleClass => RoleClass.OnCheckMurderAsTarget(killer, target) == true);
    }

    /// <summary>
    /// Builds Modified GameOptions
    /// </summary>
    public static void BuildCustomGameOptions(this PlayerControl player, ref IGameOptions opt)
    {


        var playerSubRoles = player.GetCustomSubRoles();

        if (playerSubRoles.Any())
            foreach (var subRole in playerSubRoles.ToArray())
            {
                switch (subRole)
                {
                    case CustomRoles.Watcher:
                        break;
                    case CustomRoles.Flashman:
                        //CustomRoles.Flashman.SetSpeed(player.PlayerId, false);
                        break;
                    case CustomRoles.Torch:
                        //Torch.ApplyGameOptions(opt);
                        break;
                    case CustomRoles.Reach:
                        //Reach.ApplyGameOptions(opt);
                        break;
                }
            }

        // Add-ons
        //if (Glow.IsEnable) Glow.ApplyGameOptions(opt, player); //keep this at last
        //if (Ghoul.IsEnable) Ghoul.ApplyGameOptions(player);
    }

    /// <summary>
    /// Check Murder as Killer in target
    /// </summary>
    public static bool OnCheckMurder(ref PlayerControl killer, ref PlayerControl target, ref bool __state)
    {
        if (killer == target) return true;
        var canceled = false;

        //TODOvar killerSubRoles = killer.GetCustomSubRoles();

        // If Target is possessed by Dollmaster swap controllers.
        //target = DollMaster.SwapPlayerInfo(target);

        Logger.Info("Start", "PlagueBearer.CheckAndInfect");

        /*if (PlagueBearer.HasEnabled)
        {
            PlagueBearer.CheckAndInfect(killer, target);
        }*/

        Logger.Info("Start", "ForcedCheckMurderAsKiller");

        // Forced check


        Logger.Info("Start", "OnCheckMurder.RpcCheckAndMurder");

        // Check in target
        if (killer.RpcCheckAndMurder(target, true) == false)
        {
            __state = true;
            Logger.Info("Cancels because target cancel kill", "OnCheckMurder.RpcCheckAndMurder");
            return false;
        }

        Logger.Info("Start foreach", "KillerSubRoles");


        Logger.Info("Start", "OnCheckMurderAsKiller");

        // Check murder as killer

        // Swap controllers if Sheriff shots Dollmasters main body.



        // Check if killer is a true killing role and Target is possessed by Dollmaster

        if (canceled)
            return false;



        return true;
    }
    /// <summary>
    /// Tasks after killer murder target
    /// </summary>
    public static void OnMurderPlayer(PlayerControl killer, PlayerControl target, bool inMeeting)
    {
        // ############-INFO-##############
        // When using this code, keep in mind that killer and target can be equal (Suicide)
        // And the player can also die during the Meeting
        // ################################

        //TODOPlayerControl trueDMKiller = killer; // Save real killer.


        //TODOvar killerSubRoles = killer.GetCustomSubRoles();
        //TODOvar targetSubRoles = target.GetCustomSubRoles();

        // Check suicide
        //TODOvar isSuicide = killer.PlayerId == target.PlayerId;

        // target was murder by killer




        // Check dead body for others roles
        CheckDeadBody(killer, target, inMeeting);

        // Check Lovers Suicide
    }

    /// <summary>
    /// Check if this task is marked by a role and do something.
    /// </summary>


    public static HashSet<Action<PlayerControl, PlayerControl, bool>> CheckDeadBodyOthers = [];
    /// <summary>
    /// If the role need check a present dead body
    /// </summary>
    public static void CheckDeadBody(PlayerControl killer, PlayerControl deadBody, bool inMeeting)
    {
        if (!CheckDeadBodyOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var checkDeadBodyOthers in CheckDeadBodyOthers.ToArray())
        {
            checkDeadBodyOthers(killer, deadBody, inMeeting);
        }
    }

    public static HashSet<Action<PlayerControl>> OnFixedUpdateOthers = [];
    /// <summary>
    /// Function always called in a task turn
    /// For interfering with other roles
    /// Registered with OnFixedUpdateOthers+= at initialization
    /// </summary>
    public static void OnFixedUpdate(PlayerControl player)
    {

        if (!OnFixedUpdateOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var onFixedUpdate in OnFixedUpdateOthers.ToArray())
        {
            onFixedUpdate(player);
        }
    }
    public static HashSet<Action<PlayerControl>> OnFixedUpdateLowLoadOthers = [];
    public static void OnFixedUpdateLowLoad(PlayerControl player)
    {

        if (!OnFixedUpdateLowLoadOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var onFixedUpdateLowLoad in OnFixedUpdateLowLoadOthers.ToArray())
        {
            onFixedUpdateLowLoad(player);
        }
    }

    /// <summary>
    /// When others players on entered to vent
    /// </summary>

    private static HashSet<Func<PlayerControl, PlayerControl, bool, string>> MarkOthers = [];
    /// <summary>
    /// If seer == seen then GetMarkOthers called from FixedUpadte or MeetingHud or NotifyRoles
    /// </summary>
    public static string GetMarkOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        var sb = new StringBuilder(100);
        foreach (var marker in MarkOthers)
        {
            sb.Append(marker(seer, seen, isForMeeting));
        }
        return sb.ToString();
    }

    private static HashSet<Func<PlayerControl, PlayerControl, bool, bool, string>> LowerOthers = [];
    /// <summary>
    /// If seer == seen then GetMarkOthers called from FixedUpadte or NotifyRoles
    /// </summary>
    public static string GetLowerTextOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false, bool isForHud = false)
    {
        var sb = new StringBuilder(100);
        foreach (var lower in LowerOthers)
        {
            sb.Append(lower(seer, seen, isForMeeting, isForHud));
        }
        return sb.ToString();
    }

    private static HashSet<Func<PlayerControl, PlayerControl, bool, string>> SuffixOthers = [];
    /// <summary>
    /// If seer == seen then GetMarkOthers called from FixedUpadte or NotifyRoles
    /// </summary>
    public static string GetSuffixOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {

        var sb = new StringBuilder(100);
        foreach (var suffix in SuffixOthers)
        {
            sb.Append(suffix(seer, seen, isForMeeting));
        }
        return sb.ToString();
    }

    public static void Initialize()
    {
        OtherCollectionsSet = false;
        OnFixedUpdateOthers.Clear();
        OnFixedUpdateLowLoadOthers.Clear();
        CheckDeadBodyOthers.Clear();
    }
}