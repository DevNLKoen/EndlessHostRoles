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
        CustomRoles.Swiftclaw,
        CustomRoles.Cherokious,
        CustomRoles.Chemist,
        CustomRoles.Shifter,
        CustomRoles.Evolver,
        CustomRoles.Necromancer,
        CustomRoles.Deathknight,

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
        CustomRoles.Potato or
        CustomRoles.Runner;

    public static RoleBase GetRoleClass(this CustomRoles role)
    {
        var roleClass = role switch
        {
            // Roles that use the same code as another role need to be handled here
            CustomRoles.Nuker => new Bomber(),
            CustomRoles.Undertaker => new Assassin(),
            CustomRoles.Chameleon => new Swooper(),
            CustomRoles.BloodKnight => new Wildling(),
            CustomRoles.HexMaster => new Witch(),
            CustomRoles.Imitator => new Greedier(),
            CustomRoles.Jinx => new CursedWolf(),
            CustomRoles.Juggernaut => new Sans(),
            CustomRoles.Medusa => new Cleaner(),
            CustomRoles.Poisoner => new Vampire(),
            CustomRoles.Reckless => new Sans(),
            CustomRoles.Ritualist => new EvilDiviner(),

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
            CustomRoles.Sniper => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Jester => Jester.JesterCanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Mayor => Mayor.MayorHasPortableButton.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Vulture => Vulture.CanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Opportunist => Opportunist.CanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Vindicator => CustomRoles.Impostor,
            CustomRoles.SabotageMaster => CustomRoles.Engineer,
            CustomRoles.Mafia => Options.LegacyMafia.GetBool() ? CustomRoles.Shapeshifter : CustomRoles.Impostor,
            CustomRoles.Terrorist => CustomRoles.Engineer,
            CustomRoles.Executioner => Executioner.CRoleChangeRoles[Executioner.ChangeRolesAfterTargetKilled.GetValue()].GetVNRole(checkDesyncRole: true),
            CustomRoles.Lawyer => CustomRoles.Crewmate,
            CustomRoles.NiceSwapper => CustomRoles.Crewmate,
            CustomRoles.Vampire => CustomRoles.Impostor,
            CustomRoles.BountyHunter => CustomRoles.Impostor,
            CustomRoles.Trickster => CustomRoles.Impostor,
            CustomRoles.Witch => CustomRoles.Impostor,
            CustomRoles.Agitater => CustomRoles.Impostor,
            CustomRoles.EvilDiviner => CustomRoles.Impostor,
            CustomRoles.Wildling => Wildling.CanShapeshiftOpt.GetBool() ? CustomRoles.Shapeshifter : CustomRoles.Impostor,
            CustomRoles.Morphling => CustomRoles.Shapeshifter,
            CustomRoles.Warlock => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.SerialKiller => CustomRoles.Impostor,
            CustomRoles.FireWorks => CustomRoles.Shapeshifter,
            CustomRoles.Inhibitor => CustomRoles.Impostor,
            CustomRoles.Kidnapper => CustomRoles.Shapeshifter,
            CustomRoles.Augmenter => CustomRoles.Shapeshifter,
            CustomRoles.Ventriloquist => CustomRoles.Impostor,
            CustomRoles.Echo => CustomRoles.Shapeshifter,
            CustomRoles.Abyssbringer => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Overheat => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Generator => CustomRoles.Shapeshifter,
            CustomRoles.Blackmailer => CustomRoles.Impostor,
            CustomRoles.Commander => CustomRoles.Shapeshifter,
            CustomRoles.Freezer => CustomRoles.Shapeshifter,
            CustomRoles.Changeling => CustomRoles.Shapeshifter,
            CustomRoles.Swapster => CustomRoles.Shapeshifter,
            CustomRoles.Kamikaze => CustomRoles.Impostor,
            CustomRoles.Librarian => CustomRoles.Shapeshifter,
            CustomRoles.Cantankerous => CustomRoles.Impostor,
            CustomRoles.Swiftclaw => CustomRoles.Impostor,
            CustomRoles.YinYanger => CustomRoles.Impostor,
            CustomRoles.Duellist => CustomRoles.Shapeshifter,
            CustomRoles.Consort => CustomRoles.Impostor,
            CustomRoles.Mafioso => CustomRoles.Impostor,
            CustomRoles.Chronomancer => CustomRoles.Impostor,
            CustomRoles.Nullifier => CustomRoles.Impostor,
            CustomRoles.Stealth => CustomRoles.Impostor,
            CustomRoles.Penguin => CustomRoles.Impostor,
            CustomRoles.Sapper => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Mastermind => CustomRoles.Impostor,
            CustomRoles.RiftMaker => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Gambler => CustomRoles.Impostor,
            CustomRoles.Hitman => CustomRoles.Shapeshifter,
            CustomRoles.Saboteur => CustomRoles.Impostor,
            CustomRoles.Doctor => CustomRoles.Scientist,
            CustomRoles.Puppeteer => CustomRoles.Impostor,
            CustomRoles.TimeThief => CustomRoles.Impostor,
            CustomRoles.EvilTracker => CustomRoles.Shapeshifter,
            CustomRoles.Miner => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Twister => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.SuperStar => CustomRoles.Crewmate,
            CustomRoles.Hacker => CustomRoles.Shapeshifter,
            CustomRoles.Visionary => CustomRoles.Shapeshifter,
            CustomRoles.Assassin => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
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
            CustomRoles.Zombie => CustomRoles.Impostor,
            CustomRoles.Mario => CustomRoles.Engineer,
            CustomRoles.AntiAdminer => AntiAdminer.EnableExtraAbility.GetBool() ? CustomRoles.Shapeshifter : CustomRoles.Impostor,
            CustomRoles.Sans => CustomRoles.Impostor,
            CustomRoles.Bomber => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Nuker => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.BoobyTrap => CustomRoles.Impostor,
            CustomRoles.Scavenger => CustomRoles.Impostor,
            CustomRoles.Veteran => UsePets ? CustomRoles.Crewmate : CustomRoles.Engineer,
            CustomRoles.Capitalism => CustomRoles.Impostor,
            CustomRoles.Grenadier => UsePets ? CustomRoles.Crewmate : CustomRoles.Engineer,
            CustomRoles.Gangster => CustomRoles.Impostor,
            CustomRoles.Cleaner => CustomRoles.Impostor,
            CustomRoles.Konan => CustomRoles.Crewmate,
            CustomRoles.BallLightning => CustomRoles.Impostor,
            CustomRoles.Greedier => CustomRoles.Impostor,
            CustomRoles.Workaholic => CustomRoles.Engineer,
            CustomRoles.CursedWolf => CustomRoles.Impostor,
            CustomRoles.Collector => CustomRoles.Crewmate,
            CustomRoles.ImperiusCurse => CustomRoles.Shapeshifter,
            CustomRoles.QuickShooter => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Eraser => CustomRoles.Impostor,
            CustomRoles.OverKiller => CustomRoles.Impostor,
            CustomRoles.Hangman => CustomRoles.Shapeshifter,
            CustomRoles.Sunnyboy => CustomRoles.Scientist,
            CustomRoles.Phantasm => Options.PhantomCanVent.GetBool() ? CustomRoles.Engineer : CustomRoles.Crewmate,
            CustomRoles.Councillor => CustomRoles.Impostor,
            CustomRoles.Bard => CustomRoles.Impostor,
            CustomRoles.Swooper => CustomRoles.Impostor,
            CustomRoles.Crewpostor => CustomRoles.Engineer,
            CustomRoles.Cherokious => CustomRoles.Engineer,
            CustomRoles.Disperser => UsePets ? CustomRoles.Impostor : CustomRoles.Shapeshifter,
            CustomRoles.Camouflager => CustomRoles.Shapeshifter,
            CustomRoles.Dazzler => CustomRoles.Shapeshifter,
            CustomRoles.Devourer => CustomRoles.Shapeshifter,
            CustomRoles.Deathpact => CustomRoles.Shapeshifter,
            CustomRoles.Tracker => CustomRoles.Crewmate,
            CustomRoles.Deathknight => CustomRoles.Crewmate,
            CustomRoles.Chameleon => CustomRoles.Engineer,
            CustomRoles.Lurker => CustomRoles.Impostor,
            CustomRoles.Doomsayer => CustomRoles.Crewmate,
            CustomRoles.Godfather => CustomRoles.Impostor,
            CustomRoles.Silencer => Silencer.SilenceMode.GetValue() == 1 ? CustomRoles.Shapeshifter : CustomRoles.Impostor,

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
            // Speedrun
            CustomRoles.Runner => RoleTypes.Crewmate,
            // Standard
            CustomRoles.Executioner => Executioner.CRoleChangeRoles[Executioner.ChangeRolesAfterTargetKilled.GetValue()].GetDYRole(),
            CustomRoles.Sheriff => UsePets && Sheriff.UsePet.GetBool() ? RoleTypes.GuardianAngel : RoleTypes.Impostor,
            CustomRoles.Refugee => RoleTypes.Impostor,
            CustomRoles.Amnesiac => RoleTypes.Impostor,
            CustomRoles.Agitater => RoleTypes.Impostor,
            CustomRoles.Arsonist => RoleTypes.Impostor,
            CustomRoles.Jackal => RoleTypes.Impostor,
            CustomRoles.Medusa => RoleTypes.Impostor,
            CustomRoles.Sidekick => RoleTypes.Impostor,
            CustomRoles.Innocent => RoleTypes.Impostor,
            CustomRoles.Pelican => RoleTypes.Impostor,
            CustomRoles.Pursuer => RoleTypes.Impostor,
            CustomRoles.Revolutionist => RoleTypes.Impostor,
            CustomRoles.FFF => RoleTypes.Impostor,
            CustomRoles.Medic => UsePets && Medic.UsePet.GetBool() ? RoleTypes.GuardianAngel : RoleTypes.Impostor,
            CustomRoles.HexMaster => RoleTypes.Impostor,
            CustomRoles.Glitch => RoleTypes.Impostor,
            CustomRoles.Juggernaut => RoleTypes.Impostor,
            CustomRoles.Jinx => RoleTypes.Impostor,
            CustomRoles.DarkHide => RoleTypes.Impostor,
            CustomRoles.Provocateur => RoleTypes.Impostor,
            CustomRoles.BloodKnight => RoleTypes.Impostor,
            CustomRoles.Poisoner => RoleTypes.Impostor,
            CustomRoles.NSerialKiller => RoleTypes.Impostor,
            CustomRoles.RouleteGrandeur => RoleTypes.Impostor,
            CustomRoles.Nonplus => RoleTypes.Impostor,
            CustomRoles.Tremor => RoleTypes.Impostor,
            CustomRoles.Evolver => RoleTypes.Impostor,
            CustomRoles.Rogue => RoleTypes.Impostor,
            CustomRoles.Patroller => RoleTypes.Impostor,
            CustomRoles.Simon => RoleTypes.Impostor,
            CustomRoles.Chemist => RoleTypes.Impostor,
            CustomRoles.Samurai => RoleTypes.Impostor,
            CustomRoles.Bargainer => RoleTypes.Impostor,
            CustomRoles.Tiger => RoleTypes.Impostor,
            CustomRoles.SoulHunter => RoleTypes.Impostor,
            CustomRoles.Enderman => RoleTypes.Impostor,
            CustomRoles.Mycologist => RoleTypes.Impostor,
            CustomRoles.Bubble => RoleTypes.Impostor,
            CustomRoles.Hookshot => RoleTypes.Impostor,
            CustomRoles.Sprayer => RoleTypes.Impostor,
            CustomRoles.PlagueDoctor => RoleTypes.Impostor,
            CustomRoles.Postman => RoleTypes.Impostor,
            CustomRoles.SchrodingersCat => RoleTypes.Impostor,
            CustomRoles.Shifter => RoleTypes.Impostor,
            CustomRoles.Impartial => RoleTypes.Impostor,
            CustomRoles.Predator => RoleTypes.Impostor,
            CustomRoles.Reckless => RoleTypes.Impostor,
            CustomRoles.Magician => RoleTypes.Impostor,
            CustomRoles.WeaponMaster => RoleTypes.Impostor,
            CustomRoles.Pyromaniac => RoleTypes.Impostor,
            CustomRoles.Eclipse => RoleTypes.Impostor,
            CustomRoles.Vengeance => RoleTypes.Impostor,
            CustomRoles.HeadHunter => RoleTypes.Impostor,
            CustomRoles.Imitator => RoleTypes.Impostor,
            CustomRoles.Werewolf => RoleTypes.Impostor,
            CustomRoles.Bandit => RoleTypes.Impostor,
            CustomRoles.Maverick => RoleTypes.Impostor,
            CustomRoles.Parasite => RoleTypes.Impostor,
            CustomRoles.Totocalcio => RoleTypes.Impostor,
            CustomRoles.Romantic => RoleTypes.Impostor,
            CustomRoles.VengefulRomantic => RoleTypes.Impostor,
            CustomRoles.RuthlessRomantic => RoleTypes.Impostor,
            CustomRoles.Succubus => RoleTypes.Impostor,
            CustomRoles.Necromancer => RoleTypes.Impostor,
            CustomRoles.Virus => RoleTypes.Impostor,
            CustomRoles.Ritualist => RoleTypes.Impostor,
            CustomRoles.Pickpocket => RoleTypes.Impostor,
            CustomRoles.Traitor => RoleTypes.Impostor,
            CustomRoles.PlagueBearer => RoleTypes.Impostor,
            CustomRoles.Pestilence => RoleTypes.Impostor,
            CustomRoles.Spiritcaller => RoleTypes.Impostor,
            CustomRoles.Doppelganger => RoleTypes.Impostor,
            _ => RoleTypes.GuardianAngel
        };
    }

    public static bool IsAdditionRole(this CustomRoles role) => role > CustomRoles.NotAssigned;

    public static bool IsNonNK(this CustomRoles role, bool check = false) => (!check && role == CustomRoles.Arsonist && CanCheck && Options.IsLoaded && Options.ArsonistCanIgniteAnytime != null && !Options.ArsonistCanIgniteAnytime.GetBool()) || role is
        CustomRoles.Jester or
        CustomRoles.Postman or
        CustomRoles.Shifter or
        CustomRoles.SchrodingersCat or
        CustomRoles.Impartial or
        CustomRoles.Predator or
        CustomRoles.SoulHunter or
        CustomRoles.Terrorist or
        CustomRoles.Opportunist or
        CustomRoles.Executioner or
        CustomRoles.Mario or
        CustomRoles.Lawyer or
        CustomRoles.God or
        CustomRoles.Amnesiac or
        CustomRoles.Innocent or
        CustomRoles.Vulture or
        CustomRoles.Pursuer or
        CustomRoles.Revolutionist or
        CustomRoles.Provocateur or
        CustomRoles.FFF or
        CustomRoles.Workaholic or
        CustomRoles.Collector or
        CustomRoles.Sunnyboy or
        CustomRoles.Maverick or
        CustomRoles.Phantasm or
        CustomRoles.Totocalcio or
        CustomRoles.Romantic or
        CustomRoles.VengefulRomantic or
        CustomRoles.Doomsayer or
        CustomRoles.Deathknight;

    public static bool IsNK(this CustomRoles role, bool check = false) => (role == CustomRoles.Arsonist && (check || !CanCheck || !Options.IsLoaded || Options.ArsonistCanIgniteAnytime == null || Options.ArsonistCanIgniteAnytime.GetBool())) || role is
        CustomRoles.Jackal or
        CustomRoles.Glitch or
        CustomRoles.Sidekick or
        CustomRoles.HexMaster or
        CustomRoles.Doppelganger or
        CustomRoles.Succubus or
        CustomRoles.Crewpostor or
        CustomRoles.Cherokious or
        CustomRoles.Necromancer or
        CustomRoles.Agitater or
        CustomRoles.Bandit or
        CustomRoles.Medusa or
        CustomRoles.Pelican or
        CustomRoles.Convict or
        CustomRoles.DarkHide or
        CustomRoles.Juggernaut or
        CustomRoles.Jinx or
        CustomRoles.Poisoner or
        CustomRoles.Refugee or
        CustomRoles.Simon or
        CustomRoles.Patroller or
        CustomRoles.Rogue or
        CustomRoles.Parasite or
        CustomRoles.NSerialKiller or
        CustomRoles.RouleteGrandeur or
        CustomRoles.Nonplus or
        CustomRoles.Tremor or
        CustomRoles.Evolver or
        CustomRoles.Chemist or
        CustomRoles.Samurai or
        CustomRoles.Bargainer or
        CustomRoles.Tiger or
        CustomRoles.Enderman or
        CustomRoles.Mycologist or
        CustomRoles.Bubble or
        CustomRoles.Hookshot or
        CustomRoles.Sprayer or
        CustomRoles.Magician or
        CustomRoles.WeaponMaster or
        CustomRoles.Reckless or
        CustomRoles.Eclipse or
        CustomRoles.Pyromaniac or
        CustomRoles.Vengeance or
        CustomRoles.HeadHunter or
        CustomRoles.Imitator or
        CustomRoles.Werewolf or
        CustomRoles.Ritualist or
        CustomRoles.Pickpocket or
        CustomRoles.PlagueDoctor or
        CustomRoles.Traitor or
        CustomRoles.Virus or
        CustomRoles.BloodKnight or
        CustomRoles.Spiritcaller or
        CustomRoles.RuthlessRomantic or
        CustomRoles.PlagueBearer or
        CustomRoles.Pestilence;

    public static bool IsSnitchTarget(this CustomRoles role) => role.IsNK() || role.Is(Team.Impostor);

    public static bool IsNE(this CustomRoles role) => role is
        CustomRoles.Jester or
        CustomRoles.Arsonist or
        CustomRoles.Executioner or
        CustomRoles.Doomsayer or
        CustomRoles.Innocent;

    public static bool IsNB(this CustomRoles role) => role is
        CustomRoles.Opportunist or
        CustomRoles.Lawyer or
        CustomRoles.Amnesiac or
        CustomRoles.God or
        CustomRoles.Postman or
        CustomRoles.SchrodingersCat or
        CustomRoles.Predator or
        CustomRoles.Pursuer or
        CustomRoles.FFF or
        CustomRoles.Sunnyboy or
        CustomRoles.Maverick or
        CustomRoles.Romantic or
        CustomRoles.VengefulRomantic or
        CustomRoles.Totocalcio;

    public static bool IsNC(this CustomRoles role) => role is
        CustomRoles.Mario or
        CustomRoles.Shifter or
        CustomRoles.Terrorist or
        CustomRoles.Revolutionist or
        CustomRoles.Impartial or
        CustomRoles.Vulture or
        CustomRoles.Phantasm or
        CustomRoles.Workaholic or
        CustomRoles.Collector or
        CustomRoles.Provocateur;

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
        CustomRoles.Godfather or
        CustomRoles.EvilDiviner or
        CustomRoles.Wildling or
        CustomRoles.Silencer or
        CustomRoles.Morphling or
        CustomRoles.BountyHunter or
        CustomRoles.Vampire or
        CustomRoles.Witch or
        CustomRoles.Vindicator or
        CustomRoles.Zombie or
        CustomRoles.Warlock or
        CustomRoles.Assassin or
        CustomRoles.Undertaker or
        CustomRoles.Hacker or
        CustomRoles.Visionary or
        CustomRoles.Miner or
        CustomRoles.Escapee or
        CustomRoles.SerialKiller or
        CustomRoles.Overheat or
        CustomRoles.Underdog or
        CustomRoles.Abyssbringer or
        CustomRoles.Echo or
        CustomRoles.Ventriloquist or
        CustomRoles.Augmenter or
        CustomRoles.Inhibitor or
        CustomRoles.Kidnapper or
        CustomRoles.Generator or
        CustomRoles.Blackmailer or
        CustomRoles.Commander or
        CustomRoles.Freezer or
        CustomRoles.Changeling or
        CustomRoles.Swapster or
        CustomRoles.Kamikaze or
        CustomRoles.Librarian or
        CustomRoles.Cantankerous or
        CustomRoles.Swiftclaw or
        CustomRoles.Duellist or
        CustomRoles.YinYanger or
        CustomRoles.Consort or
        CustomRoles.Mafioso or
        CustomRoles.Nullifier or
        CustomRoles.Chronomancer or
        CustomRoles.Stealth or
        CustomRoles.Penguin or
        CustomRoles.Sapper or
        CustomRoles.Hitman or
        CustomRoles.RiftMaker or
        CustomRoles.Mastermind or
        CustomRoles.Gambler or
        CustomRoles.Councillor or
        CustomRoles.Saboteur or
        CustomRoles.Puppeteer or
        CustomRoles.TimeThief or
        CustomRoles.Trickster or
        CustomRoles.Mafia or
        CustomRoles.Minimalism or
        CustomRoles.FireWorks or
        CustomRoles.Sniper or
        CustomRoles.EvilTracker or
        CustomRoles.EvilGuesser or
        CustomRoles.AntiAdminer or
        CustomRoles.Sans or
        CustomRoles.Bomber or
        CustomRoles.Nuker or
        CustomRoles.Scavenger or
        CustomRoles.BoobyTrap or
        CustomRoles.Capitalism or
        CustomRoles.Gangster or
        CustomRoles.Cleaner or
        CustomRoles.BallLightning or
        CustomRoles.Greedier or
        CustomRoles.CursedWolf or
        CustomRoles.ImperiusCurse or
        CustomRoles.QuickShooter or
        CustomRoles.Eraser or
        CustomRoles.OverKiller or
        CustomRoles.Hangman or
        CustomRoles.Bard or
        CustomRoles.Swooper or
        CustomRoles.Disperser or
        CustomRoles.Dazzler or
        CustomRoles.Deathpact or
        CustomRoles.Devourer or
        CustomRoles.Camouflager or
        CustomRoles.Twister or
        CustomRoles.Lurker;

    public static bool IsNeutral(this CustomRoles role, bool check = false) => role.IsNK(check: check) || role.IsNonNK(check: check);

    public static bool IsAbleToBeSidekicked(this CustomRoles role) => !role.IsImpostor() && !role.IsRecruitingRole() && role is not
        CustomRoles.Deathknight and not
        CustomRoles.Gangster;

    public static bool IsEvilAddon(this CustomRoles role) => role is
        CustomRoles.Madmate or
        CustomRoles.Egoist or
        CustomRoles.Charmed or
        CustomRoles.Recruit or
        CustomRoles.Contagious or
        CustomRoles.Rascal;

    public static bool IsRecruitingRole(this CustomRoles role) => role is
        CustomRoles.Jackal or
        CustomRoles.Succubus or
        CustomRoles.Necromancer or
        CustomRoles.Virus or
        CustomRoles.Spiritcaller;

    public static bool IsMadmate(this CustomRoles role) => role is
        CustomRoles.Crewpostor or
        CustomRoles.Convict or
        CustomRoles.Refugee or
        CustomRoles.Parasite;

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

        CustomRoles.Refugee => true,
        CustomRoles.Necromancer => true,
        CustomRoles.Deathknight => true,

        // Speedrun
        CustomRoles.Runner => true,

        _ => false
    };

    public static bool CancelsVote(this CustomRoles role) => role switch
    {
        CustomRoles.Godfather when Options.GodfatherCancelVote.GetBool() => true,
        _ => false
    };

    public static bool OnlySpawnsWithPets(this CustomRoles role) => !(Options.UseUnshiftTrigger.GetBool() && (!role.IsNeutral() || Options.UseUnshiftTriggerForNKs.GetBool()) && role.SimpleAbilityTrigger() && role != CustomRoles.Chemist) && OnlySpawnsWithPetsRoleList.Contains(role);

    public static bool NeedUpdateOnLights(this CustomRoles role) => (!role.UsesPetInsteadOfKill()) && (role.GetDYRole() != RoleTypes.GuardianAngel || role is
        CustomRoles.Convict or
        CustomRoles.Parasite or
        CustomRoles.Refugee or
        CustomRoles.Saboteur or
        CustomRoles.Inhibitor or
        CustomRoles.Gambler);

    public static bool IsBetrayalAddon(this CustomRoles role) => role is
        CustomRoles.Charmed or
        CustomRoles.Recruit or
        CustomRoles.Contagious or
        CustomRoles.Lovers or
        CustomRoles.Madmate or
        CustomRoles.Undead or
        CustomRoles.Egoist;

    public static bool IsImpOnlyAddon(this CustomRoles role) => Options.GroupedAddons[AddonTypes.ImpOnly].Contains(role);

    public static bool IsTaskBasedCrewmate(this CustomRoles role) => role is
        CustomRoles.Mayor;

    public static bool ForceCancelShapeshift(this CustomRoles role) => role is
        CustomRoles.Echo or
        CustomRoles.Hangman or
        CustomRoles.Generator;

    public static bool IsNoAnimationShifter(this CustomRoles role) => role is
        CustomRoles.Echo;

    public static bool SimpleAbilityTrigger(this CustomRoles role) => role is
        CustomRoles.Warlock or
        CustomRoles.Swiftclaw or
        CustomRoles.Undertaker or
        CustomRoles.Abyssbringer or
        CustomRoles.Bomber or
        CustomRoles.Nuker or
        CustomRoles.Camouflager or
        CustomRoles.Disperser or
        CustomRoles.Escapee or
        CustomRoles.FireWorks or
        CustomRoles.Librarian or
        CustomRoles.Miner or
        CustomRoles.RiftMaker or
        CustomRoles.Assassin or
        CustomRoles.QuickShooter or
        CustomRoles.Sapper or
        CustomRoles.Sniper or
        CustomRoles.Twister or
        CustomRoles.RouleteGrandeur or
        CustomRoles.Enderman or
        CustomRoles.Hookshot or
        CustomRoles.Mycologist or
        CustomRoles.Magician or
        CustomRoles.Sprayer or
        CustomRoles.Werewolf or
        CustomRoles.WeaponMaster or
        CustomRoles.Tiger or
        CustomRoles.Bargainer or
        CustomRoles.Chemist or
        CustomRoles.Simon or
        CustomRoles.Patroller;

    public static bool CheckAddonConflict(CustomRoles role, PlayerControl pc) => role.IsAdditionRole() && (!Main.NeverSpawnTogetherCombos.TryGetValue(OptionItem.CurrentPreset, out var neverList) || !neverList.TryGetValue(pc.GetCustomRole(), out var bannedAddonList) || !bannedAddonList.Contains(role)) && pc.GetCustomRole() is not CustomRoles.GuardianAngelTOZ and not CustomRoles.God && !pc.Is(CustomRoles.Madmate) && !pc.Is(CustomRoles.GM) && role is not CustomRoles.Lovers && (!pc.HasSubRole() || pc.GetCustomSubRoles().Count < Options.NoLimitAddonsNumMax.GetInt()) && (!Options.AddonCanBeSettings.TryGetValue(role, out var o) || ((o.Imp.GetBool() || !pc.GetCustomRole().IsImpostor()) && (o.Neutral.GetBool() || !pc.GetCustomRole().IsNeutral()) && (o.Crew.GetBool() || !pc.IsCrewmate()))) && (!role.IsImpOnlyAddon() || pc.IsImpostor()) && role switch
    {
        CustomRoles.Magnet when pc.Is(Team.Impostor) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Magnet) => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Amnesiac) && Amnesiac.RememberMode.GetValue() == 1 => false,
        CustomRoles.Rookie when !pc.CanUseKillButton() => false,
        CustomRoles.Energetic when !Options.UsePets.GetBool() => false,
        CustomRoles.Madmate when pc.Is(CustomRoles.Sidekick) => false,
        CustomRoles.Autopsy when pc.Is(CustomRoles.Doctor) || pc.Is(CustomRoles.Scientist) || pc.Is(CustomRoles.ScientistTOZ) || pc.Is(CustomRoles.Sunnyboy) => false,
        CustomRoles.Necroview when pc.Is(CustomRoles.Doctor) => false,
        CustomRoles.Mischievous when pc.Is(Team.Impostor) || pc.GetCustomRole().GetDYRole() != RoleTypes.Impostor || !pc.IsNeutralKiller() || Main.PlayerStates[pc.PlayerId].Role.CanUseSabotage(pc) => false,
        CustomRoles.Loyal when pc.IsCrewmate() && !Options.CrewCanBeLoyal.GetBool() => false,
        CustomRoles.Stressed when !pc.IsCrewmate() || pc.GetCustomRole().IsTasklessCrewmate() => false,
        CustomRoles.Lazy when pc.GetCustomRole().IsNeutral() || pc.IsImpostor() || (pc.GetCustomRole().IsTasklessCrewmate() && !Options.TasklessCrewCanBeLazy.GetBool()) || (pc.GetCustomRole().IsTaskBasedCrewmate() && !Options.TaskBasedCrewCanBeLazy.GetBool()) => false,
        CustomRoles.TicketsStealer or CustomRoles.Swift or CustomRoles.DeadlyQuota or CustomRoles.Damocles or CustomRoles.Mare when pc.Is(CustomRoles.Bomber) || pc.Is(CustomRoles.Nuker) || pc.Is(CustomRoles.BoobyTrap) || pc.Is(CustomRoles.Capitalism) => false,
        CustomRoles.Torch when !pc.IsCrewmate() || pc.Is(CustomRoles.Bewilder) || pc.Is(CustomRoles.Sunglasses) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Bewilder when pc.Is(CustomRoles.Torch) || pc.Is(CustomRoles.GuardianAngelTOZ) || pc.Is(CustomRoles.Sunglasses) => false,
        CustomRoles.Dynamo when pc.Is(CustomRoles.Spurt) => false,
        CustomRoles.Spurt when pc.GetCustomSubRoles().Any(x => x is CustomRoles.Dynamo or CustomRoles.Swiftclaw or CustomRoles.Giant or CustomRoles.Flashman) => false,
        CustomRoles.Sunglasses when pc.Is(CustomRoles.Torch) || pc.Is(CustomRoles.GuardianAngelTOZ) || pc.Is(CustomRoles.Bewilder) => false,
        CustomRoles.Guesser when pc.GetCustomRole() is CustomRoles.EvilGuesser or CustomRoles.NiceGuesser or CustomRoles.Doomsayer => false,
        CustomRoles.Madmate when !pc.CanBeMadmate() || pc.Is(CustomRoles.Egoist) || pc.Is(CustomRoles.Rascal) => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Detective) || pc.Is(CustomRoles.Cleaner) || pc.Is(CustomRoles.Medusa) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Youtuber when !pc.IsCrewmate() || pc.Is(CustomRoles.Madmate) || pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Egoist when !pc.IsImpostor() => false,
        CustomRoles.Damocles when pc.GetCustomRole() is CustomRoles.Bomber or CustomRoles.Nuker or CustomRoles.SerialKiller or CustomRoles.Cantankerous => false,
        CustomRoles.Damocles when !pc.CanUseKillButton() => false,
        CustomRoles.Flashman when pc.Is(CustomRoles.Swiftclaw) || pc.Is(CustomRoles.Giant) || pc.Is(CustomRoles.Spurt) => false,
        CustomRoles.Giant when pc.Is(CustomRoles.Flashman) || pc.Is(CustomRoles.Spurt) => false,
        CustomRoles.Necroview when pc.Is(CustomRoles.Visionary) => false,
        CustomRoles.Mimic when pc.Is(CustomRoles.Mafia) => false,
        CustomRoles.Rascal when !pc.IsCrewmate() => false,
        CustomRoles.TicketsStealer when pc.Is(CustomRoles.Vindicator) => false,
        CustomRoles.Bloodlust when !pc.GetCustomRole().IsCrewmate() || pc.GetCustomRole().IsTaskBasedCrewmate() || pc.GetCustomRole() is CustomRoles.Medic => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Underdog) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Inhibitor) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Saboteur) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Swift) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Mafia) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Sniper) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.FireWorks) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Swooper) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.Vampire) => false,
        CustomRoles.Lovers when pc.Is(CustomRoles.Romantic) => false,
        CustomRoles.Mare when pc.Is(CustomRoles.LastImpostor) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Mare) => false,
        CustomRoles.DualPersonality when (!pc.IsImpostor() && !pc.IsCrewmate()) || pc.Is(CustomRoles.Madmate) => false,
        CustomRoles.Loyal when (!pc.IsImpostor() && !pc.IsCrewmate()) || pc.Is(CustomRoles.Madmate) => false,
        CustomRoles.Loyal when pc.IsImpostor() && !Options.ImpCanBeLoyal.GetBool() => false,
        CustomRoles.Onbound when pc.Is(CustomRoles.SuperStar) => false,
        CustomRoles.Rascal when pc.Is(CustomRoles.SuperStar) || pc.Is(CustomRoles.Madmate) => false,
        CustomRoles.Madmate when pc.Is(CustomRoles.SuperStar) => false,
        CustomRoles.Gravestone when pc.Is(CustomRoles.SuperStar) => false,
        CustomRoles.Bait when pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Bait when pc.Is(CustomRoles.Trapper) => false,
        CustomRoles.Trapper when pc.Is(CustomRoles.Bait) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Swooper) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Vampire) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Scavenger) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Puppeteer) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Warlock) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.EvilDiviner) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Witch) => false,
        CustomRoles.Swift when pc.Is(CustomRoles.Mafia) => false,
        CustomRoles.Reach when pc.Is(CustomRoles.Mafioso) => false,
        CustomRoles.Trapper when pc.Is(CustomRoles.GuardianAngelTOZ) => false,
        CustomRoles.Reach when !pc.CanUseKillButton() => false,
        CustomRoles.Magnet when !pc.CanUseKillButton() => false,
        CustomRoles.Haste when !pc.CanUseKillButton() || !pc.CanUseImpostorVentButton() => false,
        CustomRoles.Diseased when pc.Is(CustomRoles.Antidote) => false,
        CustomRoles.Antidote when pc.Is(CustomRoles.Diseased) => false,
        CustomRoles.Flashman or CustomRoles.Giant when pc.GetCustomRole() is CustomRoles.Swooper  or CustomRoles.Chameleon => false,
        CustomRoles.Bait when pc.Is(CustomRoles.Unreportable) => false,
        CustomRoles.Busy when !pc.GetTaskState().hasTasks => false,
        CustomRoles.Truant when pc.Is(CustomRoles.SoulHunter) => false,
        CustomRoles.Nimble when !pc.IsCrewmate() => false,
        CustomRoles.Physicist when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Finder when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Noisy when !pc.IsCrewmate() || pc.GetCustomRole().GetDYRole() == RoleTypes.Impostor => false,
        CustomRoles.Unreportable when pc.Is(CustomRoles.Bait) => false,
        CustomRoles.Oblivious when pc.Is(CustomRoles.Vulture) => false,
        CustomRoles.Unlucky when pc.Is(CustomRoles.Lucky) => false,
        CustomRoles.Lucky when pc.Is(CustomRoles.Unlucky) => false,
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
    public static bool IsImpostorTeam(this CustomRoles role) => role.IsImpostor() || role == CustomRoles.Madmate;
    public static bool IsCrewmate(this CustomRoles role) => !role.IsImpostor() && !role.IsNeutral() && !role.IsMadmate();

    public static bool IsImpostorTeamV2(this CustomRoles role) => (role.IsImpostorTeam() && role != CustomRoles.Trickster && !role.IsConverted()) || role is CustomRoles.Rascal or CustomRoles.Madmate || role.IsMadmate();
    public static bool IsNeutralTeamV2(this CustomRoles role) => role.IsConverted() || (role.IsNeutral() && role != CustomRoles.Madmate);
    public static bool IsCrewmateTeamV2(this CustomRoles role) => (!role.IsImpostorTeamV2() && !role.IsNeutralTeamV2()) || (role == CustomRoles.Trickster && !role.IsConverted());

    public static bool IsConverted(this CustomRoles role) => (role == CustomRoles.Egoist) || role is
        CustomRoles.Charmed or
        CustomRoles.Recruit or
        CustomRoles.Contagious or
        CustomRoles.Undead;

    public static bool IsRevealingRole(this CustomRoles role, PlayerControl target) =>
        ((role is CustomRoles.Mayor) && Mayor.MayorRevealWhenDoneTasks.GetBool() && target.AllTasksCompleted()) ||
        ((role is CustomRoles.SuperStar) && Options.EveryOneKnowSuperStar.GetBool()) ||
        ((role is CustomRoles.Workaholic) && Workaholic.WorkaholicVisibleToEveryone.GetBool()) ||
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
        CustomRoles.Deathknight => CountTypes.Necromancer,
        CustomRoles.Parasite => CountTypes.Impostor,
        CustomRoles.Crewpostor => CountTypes.Impostor,
        CustomRoles.Refugee => CountTypes.Impostor,
        CustomRoles.DarkHide when DarkHide.SnatchesWin.GetBool() => CountTypes.Crew,
        CustomRoles.Arsonist when !Options.ArsonistKeepsGameGoing.GetBool() => CountTypes.Crew,
        CustomRoles.SchrodingersCat => SchrodingersCat.WinsWithCrewIfNotAttacked.GetBool() ? CountTypes.Crew : CountTypes.OutOfGame,
        CustomRoles.DarkHide => !DarkHide.SnatchesWin.GetBool() ? CountTypes.DarkHide : CountTypes.Crew,
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
    Bloodlust,
    Jackal,
    Doppelganger,
    Pelican,
    BloodKnight,
    Poisoner,
    Succubus,
    Necromancer,
    HexMaster,
    NSerialKiller,
    RouleteGrandeur,
    Nonplus,
    Tremor,
    Evolver,
    Rogue,
    Patroller,
    Simon,
    Chemist,
    Samurai,
    Bargainer,
    Tiger,
    Enderman,
    Mycologist,
    Bubble,
    Hookshot,
    Sprayer,
    PlagueDoctor,
    Magician,
    WeaponMaster,
    Reckless,
    Pyromaniac,
    Eclipse,
    Vengeance,
    HeadHunter,
    Imitator,
    Werewolf,
    Juggernaut,
    Agitater,
    Virus,
    DarkHide,
    Jinx,
    Ritualist,
    Pickpocket,
    Traitor,
    Medusa,
    Bandit,
    Spiritcaller,
    RuthlessRomantic,
    Pestilence,
    PlagueBearer,
    Glitch,
    Arsonist,
    Cherokious,
    Sheriff
}