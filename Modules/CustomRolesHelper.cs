using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AmongUs.GameOptions;
using TOZ.Crewmate;
using TOZ.Impostor;
using TOZ.Modules;
using TOZ.Neutral;
using UnityEngine;

namespace TOZ;

internal static class CustomRolesHelper
{
    public static bool CanCheck = false;

    private static readonly List<CustomRoles> OnlySpawnsWithPetsRoleList =
    [
        CustomRoles.Shifter,

        // Add-ons
        CustomRoles.Energetic,

        // HnS
        CustomRoles.Jet,
        CustomRoles.Dasher
    ];

    // private static readonly List<CustomRoles> ExperimentalRoleList =
    // [
    //     CustomRoles.Shifter
    // ];
    //
    // public static bool IsExperimental(this CustomRoles role) => ExperimentalRoleList.Contains(role);

    public static bool IsForOtherGameMode(this CustomRoles role) => HnSManager.AllHnSRoles.Contains(role) || role is
        CustomRoles.KB_Normal or
        CustomRoles.Killer or
        CustomRoles.Tasker or
        CustomRoles.Potato;

    public static RoleBase GetRoleClass(this CustomRoles role)
    {
        var roleClass = role switch
        {
            // Roles that use the same code as another role need to be handled here
            CustomRoles.Nuker => new Bomber(),

            // Else, the role class is the role name - if the class doesn't exist, it defaults to VanillaRole
            _ => Main.AllRoleClasses.FirstOrDefault(x => x.GetType().Name.Equals(role.ToString(), StringComparison.OrdinalIgnoreCase)) ?? new VanillaRole()
        };

        return Activator.CreateInstance(roleClass.GetType()) as RoleBase;
    }

    public static CustomRoles GetVNRole(this CustomRoles role, bool checkDesyncRole = false)
    {
        if (role.IsVanilla()) return role;
        if (checkDesyncRole && role.GetDYRole() == RoleTypes.Impostor) return CustomRoles.Impostor;
        if (Options.UsePhantomBasis.GetBool() && role.SimpleAbilityTrigger()) return CustomRoles.Phantom;
        if (Options.UseUnshiftTrigger.GetBool() && role.SimpleAbilityTrigger()) return CustomRoles.Shapeshifter;
        bool UsePets = Options.UsePets.GetBool();
        return role switch
        {
            CustomRoles.Jester => Jester.JesterCanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Mayor => Mayor.MayorHasPortableButton.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Vulture => Vulture.CanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Opportunist => Opportunist.CanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Vindicator => CustomRoles.Impostor,
            CustomRoles.SabotageMaster => CustomRoles.Engineer,
            CustomRoles.Terrorist => CustomRoles.Engineer,
            CustomRoles.Executioner => Executioner.CRoleChangeRoles[Executioner.ChangeRolesAfterTargetKilled.GetValue()].GetVNRole(checkDesyncRole: true),
            CustomRoles.Lawyer => CustomRoles.Crewmate,
            CustomRoles.NiceSwapper => CustomRoles.Crewmate,
            CustomRoles.Vampire => CustomRoles.Impostor,
            CustomRoles.Trickster => CustomRoles.Impostor,
            CustomRoles.Agitater => CustomRoles.Impostor,
            CustomRoles.Kidnapper => CustomRoles.Shapeshifter,
            CustomRoles.Chronomancer => CustomRoles.Impostor,
            CustomRoles.Sapper => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.RiftMaker => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Gambler => CustomRoles.Impostor,
            CustomRoles.Doctor => CustomRoles.Scientist,
            CustomRoles.SuperStar => CustomRoles.Crewmate,
            CustomRoles.Undertaker => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.CyberStar => CustomRoles.Crewmate,
            CustomRoles.TaskManager => CustomRoles.Crewmate,
            CustomRoles.LovingCrewmate => CustomRoles.Crewmate,
            CustomRoles.LovingImpostor => CustomRoles.Impostor,
            CustomRoles.Escapee => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.NiceGuesser => CustomRoles.Crewmate,
            CustomRoles.EvilGuesser => CustomRoles.Impostor,
            CustomRoles.Detective => CustomRoles.Crewmate,
            CustomRoles.God => CustomRoles.Crewmate,
            CustomRoles.GuardianAngelTOZ => CustomRoles.GuardianAngel,
            CustomRoles.Bomber => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Nuker => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Veteran => UsePets ? CustomRoles.Crewmate : CustomRoles.Engineer,
            CustomRoles.Grenadier => UsePets ? CustomRoles.Crewmate : CustomRoles.Engineer,
            CustomRoles.Cleaner => CustomRoles.Impostor,
            CustomRoles.Konan => CustomRoles.Crewmate,
            CustomRoles.ImperiusCurse => CustomRoles.Shapeshifter,
            CustomRoles.Sunnyboy => CustomRoles.Scientist,
            CustomRoles.Bard => CustomRoles.Impostor,
            CustomRoles.Tracker => CustomRoles.Crewmate,
            CustomRoles.Chameleon => CustomRoles.Engineer,
            CustomRoles.Doomsayer => CustomRoles.Crewmate,

            // Vanilla roles (just in case)
            CustomRoles.ImpostorTOZ => CustomRoles.Impostor,
            CustomRoles.PhantomTOZ => CustomRoles.Phantom,
            CustomRoles.ShapeshifterTOZ => CustomRoles.Shapeshifter,
            CustomRoles.CrewmateTOZ => CustomRoles.Crewmate,
            CustomRoles.EngineerTOZ => CustomRoles.Engineer,
            CustomRoles.ScientistTOZ => CustomRoles.Scientist,
            CustomRoles.TrackerTOZ => CustomRoles.Tracker,
            CustomRoles.NoisemakerTOZ => CustomRoles.Noisemaker,

            // Hide And Seek
            CustomRoles.Hider => CustomRoles.Crewmate,
            CustomRoles.Seeker => CustomRoles.Impostor,
            CustomRoles.Fox => CustomRoles.Crewmate,
            CustomRoles.Troll => CustomRoles.Crewmate,
            CustomRoles.Jumper => CustomRoles.Engineer,
            CustomRoles.Detector => CustomRoles.Crewmate,
            CustomRoles.Jet => CustomRoles.Crewmate,
            CustomRoles.Dasher => CustomRoles.Impostor,
            CustomRoles.Locator => CustomRoles.Impostor,
            CustomRoles.Venter => CustomRoles.Impostor,
            CustomRoles.Agent => CustomRoles.Impostor,
            CustomRoles.Taskinator => CustomRoles.Crewmate,

            _ => role.IsImpostor() ? CustomRoles.Impostor : CustomRoles.Crewmate
        };
    }

    public static CustomRoles GetErasedRole(this CustomRoles role)
    {
        if (role.IsVanilla()) return role;
        var vnRole = role.GetVNRole();
        if (role.GetDYRole() == RoleTypes.Impostor) vnRole = CustomRoles.Impostor;
        return vnRole switch
        {
            CustomRoles.Crewmate => CustomRoles.CrewmateTOZ,
            CustomRoles.Engineer => CustomRoles.EngineerTOZ,
            CustomRoles.Noisemaker => CustomRoles.NoisemakerTOZ,
            CustomRoles.Tracker => CustomRoles.TrackerTOZ,
            CustomRoles.Scientist => CustomRoles.ScientistTOZ,
            CustomRoles.Impostor when role.IsCrewmate() => CustomRoles.CrewmateTOZ,
            CustomRoles.Impostor => CustomRoles.ImpostorTOZ,
            CustomRoles.Phantom => CustomRoles.PhantomTOZ,
            CustomRoles.Shapeshifter => CustomRoles.ShapeshifterTOZ,
            _ => role.IsImpostor() ? CustomRoles.ImpostorTOZ : CustomRoles.CrewmateTOZ
        };
    }

    public static RoleTypes GetDYRole(this CustomRoles role, bool load = false)
    {
        if (!load && Options.UsePhantomBasis.GetBool() && Options.UsePhantomBasisForNKs.GetBool() && !role.IsImpostor() && role.SimpleAbilityTrigger()) return RoleTypes.Phantom;
        if (!load && Options.UseUnshiftTrigger.GetBool() && Options.UseUnshiftTriggerForNKs.GetBool() && !role.IsImpostor() && role.SimpleAbilityTrigger()) return RoleTypes.Shapeshifter;
        bool UsePets = !load && Options.UsePets.GetBool();
        return role switch
        {
            // SoloKombat
            CustomRoles.KB_Normal => RoleTypes.Impostor,
            // FFA
            CustomRoles.Killer => RoleTypes.Impostor,
            // Move And Stop
            CustomRoles.Tasker => RoleTypes.Crewmate,
            // Hot Potato
            CustomRoles.Potato => RoleTypes.Crewmate,
            // Standard
            CustomRoles.Executioner => Executioner.CRoleChangeRoles[Executioner.ChangeRolesAfterTargetKilled.GetValue()].GetDYRole(),
            CustomRoles.Sheriff => UsePets && Sheriff.UsePet.GetBool() ? RoleTypes.GuardianAngel : RoleTypes.Impostor,
            CustomRoles.Amnesiac => RoleTypes.Impostor,
            CustomRoles.Agitater => RoleTypes.Impostor,
            CustomRoles.Arsonist => RoleTypes.Impostor,
            CustomRoles.Sidekick => RoleTypes.Impostor,
            CustomRoles.Medic => UsePets && Medic.UsePet.GetBool() ? RoleTypes.GuardianAngel : RoleTypes.Impostor,
            CustomRoles.Glitch => RoleTypes.Impostor,
            CustomRoles.BloodKnight => RoleTypes.Impostor,
            CustomRoles.Poisoner => RoleTypes.Impostor,
            CustomRoles.NSerialKiller => RoleTypes.Impostor,
            CustomRoles.Enderman => RoleTypes.Impostor,
            CustomRoles.Sprayer => RoleTypes.Impostor,
            CustomRoles.PlagueDoctor => RoleTypes.Impostor,
            CustomRoles.Shifter => RoleTypes.Impostor,
            CustomRoles.Pyromaniac => RoleTypes.Impostor,
            CustomRoles.Werewolf => RoleTypes.Impostor,
            CustomRoles.Romantic => RoleTypes.Impostor,
            CustomRoles.VengefulRomantic => RoleTypes.Impostor,
            CustomRoles.RuthlessRomantic => RoleTypes.Impostor,
            CustomRoles.Virus => RoleTypes.Impostor,
            CustomRoles.Traitor => RoleTypes.Impostor,
            CustomRoles.PlagueBearer => RoleTypes.Impostor,
            CustomRoles.Pestilence => RoleTypes.Impostor,
            _ => RoleTypes.GuardianAngel
        };
    }

    public static bool IsAdditionRole(this CustomRoles role) => role > CustomRoles.NotAssigned;

    public static bool IsNonNK(this CustomRoles role, bool check = false) => (!check && role == CustomRoles.Arsonist && CanCheck && Options.IsLoaded && Options.ArsonistCanIgniteAnytime != null && !Options.ArsonistCanIgniteAnytime.GetBool()) || role is
        CustomRoles.Jester or
        CustomRoles.Shifter or
        CustomRoles.Terrorist or
        CustomRoles.Opportunist or
        CustomRoles.Executioner or
        CustomRoles.Lawyer or
        CustomRoles.God or
        CustomRoles.Amnesiac or
        CustomRoles.Vulture or
        CustomRoles.Sunnyboy or
        CustomRoles.Romantic or
        CustomRoles.VengefulRomantic or
        CustomRoles.Doomsayer;

    public static bool IsNK(this CustomRoles role, bool check = false) => (role == CustomRoles.Arsonist && (check || !CanCheck || !Options.IsLoaded || Options.ArsonistCanIgniteAnytime == null || Options.ArsonistCanIgniteAnytime.GetBool())) || role is
        CustomRoles.Glitch or
        CustomRoles.Sidekick or
        CustomRoles.Agitater or
        CustomRoles.Poisoner or
        CustomRoles.NSerialKiller or
        CustomRoles.Enderman or
        CustomRoles.Sprayer or
        CustomRoles.Pyromaniac or
        CustomRoles.Werewolf or
        CustomRoles.PlagueDoctor or
        CustomRoles.Traitor or
        CustomRoles.Virus or
        CustomRoles.BloodKnight or
        CustomRoles.RuthlessRomantic or
        CustomRoles.PlagueBearer or
        CustomRoles.Pestilence;

    public static bool IsSnitchTarget(this CustomRoles role) => role.IsNK() || role.Is(Team.Impostor);

    public static bool IsNE(this CustomRoles role) => role is
        CustomRoles.Jester or
        CustomRoles.Arsonist or
        CustomRoles.Executioner or
        CustomRoles.Doomsayer;

    public static bool IsNB(this CustomRoles role) => role is
        CustomRoles.Opportunist or
        CustomRoles.Lawyer or
        CustomRoles.Amnesiac or
        CustomRoles.God or
        CustomRoles.Sunnyboy or
        CustomRoles.Romantic or
        CustomRoles.VengefulRomantic;

    public static bool IsNC(this CustomRoles role) => role is
        CustomRoles.Shifter or
        CustomRoles.Terrorist or
        CustomRoles.Vulture;

    public static bool IsCK(this CustomRoles role) => role is
        CustomRoles.Veteran or
        CustomRoles.NiceGuesser or
        CustomRoles.Sheriff;

    public static bool IsImpostor(this CustomRoles role) => role is
        CustomRoles.Impostor or
        CustomRoles.ImpostorTOZ or
        CustomRoles.Phantom or
        CustomRoles.PhantomTOZ or
        CustomRoles.Shapeshifter or
        CustomRoles.ShapeshifterTOZ or
        CustomRoles.LovingImpostor or
        CustomRoles.Vampire or
        CustomRoles.Vindicator or
        CustomRoles.Undertaker or
        CustomRoles.Escapee or
        CustomRoles.Underdog or
        CustomRoles.Kidnapper or
        CustomRoles.Chronomancer or
        CustomRoles.Sapper or
        CustomRoles.RiftMaker or
        CustomRoles.Gambler or
        CustomRoles.Trickster or
        CustomRoles.EvilGuesser or
        CustomRoles.Bomber or
        CustomRoles.Nuker or
        CustomRoles.Cleaner or
        CustomRoles.ImperiusCurse or
        CustomRoles.Bard;

    public static bool IsNeutral(this CustomRoles role, bool check = false) => role.IsNK(check: check) || role.IsNonNK(check: check);

    public static bool IsEvilAddon(this CustomRoles role) => role is
        CustomRoles.Recruit or
        CustomRoles.Contagious or
        CustomRoles.Rascal;

    public static bool IsRecruitingRole(this CustomRoles role) => role is
        CustomRoles.Virus;

    public static bool IsTasklessCrewmate(this CustomRoles role) => !role.UsesPetInsteadOfKill() && role is
        CustomRoles.Sheriff or
        CustomRoles.Medic;

    public static bool PetActivatedAbility(this CustomRoles role)
    {
        if (!Options.UsePets.GetBool()) return false;
        if (role.UsesPetInsteadOfKill()) return true;

        var type = role.GetRoleClass().GetType();
        return type.GetMethod("OnPet")?.DeclaringType == type;
    }

    public static bool UsesPetInsteadOfKill(this CustomRoles role) => Options.UsePets.GetBool() && role switch
    {
        CustomRoles.Sheriff when Sheriff.UsePet.GetBool() => true,
        CustomRoles.Medic when Medic.UsePet.GetBool() => true,

        _ => false
    };

    public static bool OnlySpawnsWithPets(this CustomRoles role) => !(Options.UseUnshiftTrigger.GetBool() && (!role.IsNeutral() || Options.UseUnshiftTriggerForNKs.GetBool()) && role.SimpleAbilityTrigger()) && OnlySpawnsWithPetsRoleList.Contains(role);

    public static bool NeedUpdateOnLights(this CustomRoles role) => (!role.UsesPetInsteadOfKill()) && (role.GetDYRole() != RoleTypes.GuardianAngel || role is
        CustomRoles.Gambler);

    public static bool IsBetrayalAddon(this CustomRoles role) => role is
        CustomRoles.Recruit or
        CustomRoles.Contagious or
        CustomRoles.Lovers;

    public static bool IsImpOnlyAddon(this CustomRoles role) => Options.GroupedAddons[AddonTypes.ImpOnly].Contains(role);

    public static bool IsTaskBasedCrewmate(this CustomRoles role) => role is
        CustomRoles.Mayor;

    public static bool SimpleAbilityTrigger(this CustomRoles role) => role is
        CustomRoles.Undertaker or
        CustomRoles.Bomber or
        CustomRoles.Nuker or
        CustomRoles.Escapee or
        CustomRoles.RiftMaker or
        CustomRoles.Sapper or
        CustomRoles.Enderman or
        CustomRoles.Sprayer or
        CustomRoles.Werewolf;

    public static bool CheckAddonConflict(CustomRoles role, PlayerControl pc) => role.IsAdditionRole() && (!Main.NeverSpawnTogetherCombos.TryGetValue(OptionItem.CurrentPreset, out var neverList) || !neverList.TryGetValue(pc.GetCustomRole(), out var bannedAddonList) || !bannedAddonList.Contains(role)) && pc.GetCustomRole() is not CustomRoles.GuardianAngelTOZ and not CustomRoles.God && !pc.Is(CustomRoles.GM) && role is not CustomRoles.Lovers && (!pc.HasSubRole() || pc.GetCustomSubRoles().Count < Options.NoLimitAddonsNumMax.GetInt()) && (!Options.AddonCanBeSettings.TryGetValue(role, out var o) || ((o.Imp.GetBool() || !pc.GetCustomRole().IsImpostor()) && (o.Neutral.GetBool() || !pc.GetCustomRole().IsNeutral()) && (o.Crew.GetBool() || !pc.IsCrewmate()))) && (!role.IsImpOnlyAddon() || pc.IsImpostor()) && role switch
    {
        CustomRoles.Magnet when pc.Is(Team.Impostor) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Magnet) => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Amnesiac) && Amnesiac.RememberMode.GetValue() == 1 => false,
        CustomRoles.Rookie when !pc.CanUseKillButton() => false,
        CustomRoles.Energetic when !Options.UsePets.GetBool() => false,
        CustomRoles.Autopsy when pc.Is(CustomRoles.Doctor) || pc.Is(CustomRoles.Scientist) || pc.Is(CustomRoles.ScientistTOZ) || pc.Is(CustomRoles.Sunnyboy) => false,
        CustomRoles.Necroview when pc.Is(CustomRoles.Doctor) => false,
        CustomRoles.Mischievous when pc.Is(Team.Impostor) || pc.GetCustomRole().GetDYRole() != RoleTypes.Impostor || !pc.IsNeutralKiller() || Main.PlayerStates[pc.PlayerId].Role.CanUseSabotage(pc) => false,
        CustomRoles.Lazy when pc.GetCustomRole().IsNeutral() || pc.IsImpostor() || (pc.GetCustomRole().IsTasklessCrewmate() && !Options.TasklessCrewCanBeLazy.GetBool()) || (pc.GetCustomRole().IsTaskBasedCrewmate() && !Options.TaskBasedCrewCanBeLazy.GetBool()) => false,
        CustomRoles.Torch when !pc.IsCrewmate() || pc.Is(CustomRoles.Sunglasses) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Sunglasses when pc.Is(CustomRoles.Torch) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Guesser when pc.GetCustomRole() is CustomRoles.NiceGuesser or CustomRoles.Doomsayer => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Detective) || pc.Is(CustomRoles.Cleaner) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Damocles when pc.GetCustomRole() is CustomRoles.Bomber or CustomRoles.Nuker => false,
        CustomRoles.Damocles when !pc.CanUseKillButton() => false,
        CustomRoles.Flashman when pc.Is(CustomRoles.Giant) => false,
        CustomRoles.Giant when pc.Is(CustomRoles.Flashman) => false,
        CustomRoles.Rascal when !pc.IsCrewmate() => false,
        CustomRoles.Lovers when pc.Is(CustomRoles.Romantic) => false,
        CustomRoles.DualPersonality when (!pc.IsImpostor() && !pc.IsCrewmate()) => false,
        CustomRoles.Rascal when pc.Is(CustomRoles.SuperStar) => false,
        CustomRoles.Gravestone when pc.Is(CustomRoles.SuperStar) => false,
        CustomRoles.Bait when pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Vampire) => false,
        CustomRoles.Reach when !pc.CanUseKillButton() => false,
        CustomRoles.Magnet when !pc.CanUseKillButton() => false,
        CustomRoles.Flashman or CustomRoles.Giant when pc.GetCustomRole() is CustomRoles.Chameleon => false,
        CustomRoles.Bait when pc.Is(CustomRoles.Unreportable) => false,
        CustomRoles.Busy when !pc.GetTaskState().hasTasks => false,
        CustomRoles.Nimble when !pc.IsCrewmate() => false,
        CustomRoles.Physicist when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Finder when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Noisy when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Unreportable when pc.Is(CustomRoles.Bait) => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Vulture) => false,
        CustomRoles.Fool when pc.Is(CustomRoles.SabotageMaster) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.DoubleShot when !pc.Is(CustomRoles.EvilGuesser) && !pc.Is(CustomRoles.NiceGuesser) && !Options.GuesserMode.GetBool() => false,
        CustomRoles.DoubleShot when !pc.Is(CustomRoles.Guesser) && !pc.Is(CustomRoles.EvilGuesser) && pc.Is(CustomRoleTypes.Impostor) && !Options.ImpostorsCanGuess.GetBool() => false,
        CustomRoles.DoubleShot when !pc.Is(CustomRoles.Guesser) && !pc.Is(CustomRoles.NiceGuesser) && pc.Is(CustomRoleTypes.Crewmate) && !Options.CrewmatesCanGuess.GetBool() => false,
        CustomRoles.DoubleShot when !pc.Is(CustomRoles.Guesser) && ((pc.GetCustomRole().IsNonNK() && !Options.PassiveNeutralsCanGuess.GetBool()) || (pc.IsNeutralKiller() && !Options.NeutralKillersCanGuess.GetBool())) => false,
        _ => true
    };

    public static Team GetTeam(this CustomRoles role)
    {
        if (role.IsImpostorTeamV2()) return Team.Impostor;
        if (role.IsNeutralTeamV2()) return Team.Neutral;
        return role.IsCrewmateTeamV2() ? Team.Crewmate : Team.None;
    }

    public static bool Is(this CustomRoles role, Team team) => team switch
    {
        Team.Impostor => role.IsImpostorTeamV2(),
        Team.Neutral => role.IsNeutralTeamV2(),
        Team.Crewmate => role.IsCrewmateTeamV2(),
        Team.None => role.GetCountTypes() is CountTypes.OutOfGame or CountTypes.None || role == CustomRoles.GM,
        _ => false
    };

    public static RoleTypes GetRoleTypes(this CustomRoles role)
    {
        if (role.GetDYRole() == RoleTypes.Impostor) return RoleTypes.Impostor;
        if (Enum.TryParse<RoleTypes>(role.GetVNRole().ToString(), ignoreCase: true, out var type)) return type;
        return role.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate;
    }

    public static bool IsDesyncRole(this CustomRoles role) => role.GetDYRole() != RoleTypes.GuardianAngel;
    public static bool IsImpostorTeam(this CustomRoles role) => role.IsImpostor();
    public static bool IsCrewmate(this CustomRoles role) => !role.IsImpostor() && !role.IsNeutral();

    public static bool IsImpostorTeamV2(this CustomRoles role) => (role.IsImpostorTeam() && role != CustomRoles.Trickster && !role.IsConverted()) || role is CustomRoles.Rascal;
    public static bool IsNeutralTeamV2(this CustomRoles role) => role.IsConverted() || (role.IsNeutral());
    public static bool IsCrewmateTeamV2(this CustomRoles role) => (!role.IsImpostorTeamV2() && !role.IsNeutralTeamV2()) || (role == CustomRoles.Trickster && !role.IsConverted());

    public static bool IsConverted(this CustomRoles role) => role is
        CustomRoles.Recruit or
        CustomRoles.Contagious;

    public static bool IsRevealingRole(this CustomRoles role, PlayerControl target) =>
        ((role is CustomRoles.Mayor) && Mayor.MayorRevealWhenDoneTasks.GetBool() && target.AllTasksCompleted()) ||
        ((role is CustomRoles.SuperStar) && Options.EveryOneKnowSuperStar.GetBool()) ||
        ((role is CustomRoles.Doctor) && Options.DoctorVisibleToEveryone.GetBool()) ||
        ((role is CustomRoles.Bait) && Options.BaitNotification.GetBool());


    public static bool IsVanilla(this CustomRoles role) => role is
        CustomRoles.Crewmate or
        CustomRoles.Engineer or
        CustomRoles.Noisemaker or
        CustomRoles.Tracker or
        CustomRoles.Scientist or
        CustomRoles.GuardianAngel or
        CustomRoles.Impostor or
        CustomRoles.Phantom or
        CustomRoles.Shapeshifter;

    public static CustomRoleTypes GetCustomRoleTypes(this CustomRoles role)
    {
        CustomRoleTypes type = CustomRoleTypes.Crewmate;
        if (role.IsImpostor()) type = CustomRoleTypes.Impostor;
        if (role.IsNeutral()) type = CustomRoleTypes.Neutral;
        if (role.IsAdditionRole()) type = CustomRoleTypes.Addon;
        return type;
    }

    public static bool RoleExist(this CustomRoles role, bool countDead = false) => Main.AllPlayerControls.Any(x => x.Is(role) && (countDead || x.IsAlive()));

    public static int GetCount(this CustomRoles role)
    {
        if (role.IsVanilla())
        {
            if (Options.DisableVanillaRoles.GetBool()) return 0;
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetNumPerGame(RoleTypes.Engineer),
                CustomRoles.Noisemaker => roleOpt.GetNumPerGame(RoleTypes.Noisemaker),
                CustomRoles.Tracker => roleOpt.GetNumPerGame(RoleTypes.Tracker),
                CustomRoles.Scientist => roleOpt.GetNumPerGame(RoleTypes.Scientist),
                CustomRoles.Impostor => roleOpt.GetNumPerGame(RoleTypes.Impostor),
                CustomRoles.Phantom => roleOpt.GetNumPerGame(RoleTypes.Phantom),
                CustomRoles.Shapeshifter => roleOpt.GetNumPerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetNumPerGame(RoleTypes.GuardianAngel),
                CustomRoles.Crewmate => roleOpt.GetNumPerGame(RoleTypes.Crewmate),
                _ => 0
            };
        }

        return Options.GetRoleCount(role);
    }

    public static int GetMode(this CustomRoles role) => Options.GetRoleSpawnMode(role);

    public static float GetChance(this CustomRoles role)
    {
        if (role.IsVanilla())
        {
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetChancePerGame(RoleTypes.Engineer),
                CustomRoles.Noisemaker => roleOpt.GetChancePerGame(RoleTypes.Noisemaker),
                CustomRoles.Tracker => roleOpt.GetChancePerGame(RoleTypes.Tracker),
                CustomRoles.Scientist => roleOpt.GetChancePerGame(RoleTypes.Scientist),
                CustomRoles.Impostor => roleOpt.GetChancePerGame(RoleTypes.Impostor),
                CustomRoles.Phantom => roleOpt.GetChancePerGame(RoleTypes.Phantom),
                CustomRoles.Shapeshifter => roleOpt.GetChancePerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetChancePerGame(RoleTypes.GuardianAngel),
                CustomRoles.Crewmate => roleOpt.GetChancePerGame(RoleTypes.Crewmate),
                _ => 0
            } / 100f;
        }

        return Options.GetRoleChance(role);
    }

    public static bool IsEnable(this CustomRoles role) => role.GetCount() > 0;

    public static CountTypes GetCountTypes(this CustomRoles role) => role switch
    {
        CustomRoles.GM => CountTypes.OutOfGame,
        CustomRoles.Sidekick => CountTypes.Jackal,
        CustomRoles.Arsonist when !Options.ArsonistKeepsGameGoing.GetBool() => CountTypes.Crew,
        CustomRoles.Arsonist => Options.ArsonistKeepsGameGoing.GetBool() ? CountTypes.Arsonist : CountTypes.Crew,
        CustomRoles.Sheriff => Sheriff.KeepsGameGoing.GetBool() ? CountTypes.Sheriff : CountTypes.Crew,
        CustomRoles.Shifter => CountTypes.OutOfGame,

        _ => Enum.TryParse(role.ToString(), true, out CountTypes type)
            ? type
            : role.Is(Team.Impostor) || role == CustomRoles.Trickster
                ? CountTypes.Impostor
                : CountTypes.Crew
    };

    public static RoleOptionType GetRoleOptionType(this CustomRoles role)
    {
        if (role.IsImpostor()) return RoleOptionType.Impostor;
        if (role.IsCrewmate()) return role.GetDYRole(load: true) == RoleTypes.Impostor ? RoleOptionType.Crewmate_ImpostorBased : RoleOptionType.Crewmate_Normal;
        if (role.IsNeutral(check: true)) return role.IsNK(check: true) ? RoleOptionType.Neutral_Killing : RoleOptionType.Neutral_NonKilling;
        return RoleOptionType.Crewmate_Normal;
    }

    public static Color GetRoleOptionTypeColor(this RoleOptionType type) => type switch
    {
        RoleOptionType.Impostor => Palette.ImpostorRed,
        RoleOptionType.Crewmate_Normal => Palette.CrewmateBlue,
        RoleOptionType.Crewmate_ImpostorBased => Utils.GetRoleColor(CustomRoles.Sheriff),
        RoleOptionType.Neutral_NonKilling => Utils.GetRoleColor(CustomRoles.Sprayer),
        RoleOptionType.Neutral_Killing => Utils.GetRoleColor(CustomRoles.Traitor),
        _ => Palette.AcceptedGreen,
    };

    public static TabGroup GetTabFromOptionType(this RoleOptionType type) => type switch
    {
        RoleOptionType.Impostor => TabGroup.ImpostorRoles,
        RoleOptionType.Crewmate_Normal => TabGroup.CrewmateRoles,
        RoleOptionType.Crewmate_ImpostorBased => TabGroup.CrewmateRoles,
        RoleOptionType.Neutral_NonKilling => TabGroup.NeutralRoles,
        RoleOptionType.Neutral_Killing => TabGroup.NeutralRoles,
        _ => TabGroup.ZloosSettings
        //_ => TabGroup.OtherRoles
    };

    public static SimpleRoleOptionType GetSimpleRoleOptionType(this RoleOptionType type) => type switch
    {
        RoleOptionType.Impostor => SimpleRoleOptionType.Impostor,
        RoleOptionType.Crewmate_Normal => SimpleRoleOptionType.Crewmate,
        RoleOptionType.Crewmate_ImpostorBased => SimpleRoleOptionType.Crewmate,
        RoleOptionType.Neutral_NonKilling => SimpleRoleOptionType.NNK,
        RoleOptionType.Neutral_Killing => SimpleRoleOptionType.NK,
        _ => SimpleRoleOptionType.Crewmate
    };

    public static SimpleRoleOptionType GetSimpleRoleOptionType(this CustomRoles role)
    {
        if (role.IsImpostor()) return SimpleRoleOptionType.Impostor;
        if (role.IsCrewmate()) return SimpleRoleOptionType.Crewmate;
        if (role.IsNeutral(check: true)) return role.IsNK(check: true) ? SimpleRoleOptionType.NK : SimpleRoleOptionType.NNK;
        return SimpleRoleOptionType.Crewmate;
    }

    public static Color GetAddonTypeColor(this AddonTypes type) => type switch
    {
        AddonTypes.ImpOnly => Palette.ImpostorRed,
        AddonTypes.Helpful => Palette.CrewmateBlue,
        AddonTypes.Harmful => Utils.GetRoleColor(CustomRoles.Sprayer),
        AddonTypes.Mixed => Utils.GetRoleColor(CustomRoles.TaskManager),
        _ => Palette.CrewmateBlue
    };

    public static Color GetTeamColor(this Team team) => ColorUtility.TryParseHtmlString(team switch
    {
        Team.Crewmate => Main.CrewmateColor,
        Team.Neutral => Main.NeutralColor,
        Team.Impostor => Main.ImpostorColor,
        _ => string.Empty
    }, out var color)
        ? color
        : Color.clear;

    public static string ToColoredString(this CustomRoles role) => Utils.ColorString(Utils.GetRoleColor(role), Translator.GetString($"{role}"));
}

#pragma warning disable IDE0079
[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning restore IDE0079
public enum RoleOptionType
{
    Impostor,
    Crewmate_Normal,
    Crewmate_ImpostorBased,
    Neutral_NonKilling,
    Neutral_Killing
}

public enum SimpleRoleOptionType
{
    Crewmate,
    Impostor,
    NK,
    NNK
}

public enum AddonTypes
{
    ImpOnly,
    Helpful,
    Harmful,
    Mixed
}

public enum CustomRoleTypes
{
    Crewmate,
    Impostor,
    Neutral,
    Addon
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum CountTypes
{
    OutOfGame,
    None,
    Crew,
    Impostor,
    Jackal,
    Pelican,
    BloodKnight,
    Poisoner,
    NSerialKiller,
    Tiger,
    Enderman,
    Sprayer,
    PlagueDoctor,
    Magician,
    Reckless,
    Pyromaniac,
    HeadHunter,
    Werewolf,
    Juggernaut,
    Agitater,
    Virus,
    Jinx,
    Ritualist,
    Traitor,
    Medusa,
    RuthlessRomantic,
    Pestilence,
    PlagueBearer,
    Glitch,
    Arsonist,
    Sheriff
}