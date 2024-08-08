using System;
using TOZ.Crewmate;
using TOZ.Impostor;
using TOZ.Neutral;
using TOZ.Patches;
using HarmonyLib;
using UnityEngine;

namespace TOZ;

public static class CustomButton
{
    public static Sprite Get(string name) => Utils.LoadSprite($"TOZ.Resources.Images.Skills.{name}.png", 115f);
}

[HarmonyPriority(520)]
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudSpritePatch
{
    private static Sprite Kill;
    private static Sprite Ability;
    private static Sprite Vent;
    private static Sprite Sabotage;
    private static Sprite Pet;
    private static Sprite Report;

    private static long LastErrorTime;

    public static void Postfix(HudManager __instance)
    {
        try
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;
            if (!SetHudActivePatch.IsActive || !player.IsAlive()) return;
            if (!AmongUsClient.Instance.IsGameStarted || !Main.IntroDestroyed)
            {
                Kill = null;
                Ability = null;
                Vent = null;
                Sabotage = null;
                Pet = null;
                Report = null;
                return;
            }

            bool shapeshifting = player.IsShifted();

            if (!Kill) Kill = __instance.KillButton.graphic.sprite;
            if (!Ability) Ability = __instance.AbilityButton.graphic.sprite;
            if (!Vent) Vent = __instance.ImpostorVentButton.graphic.sprite;
            if (!Sabotage) Sabotage = __instance.SabotageButton.graphic.sprite;
            if (!Pet) Pet = __instance.PetButton.graphic.sprite;
            if (!Report) Report = __instance.ReportButton.graphic.sprite;

            Sprite newKillButton = Kill;
            Sprite newAbilityButton = Ability;
            Sprite newVentButton = Vent;
            Sprite newSabotageButton = Sabotage;
            Sprite newPetButton = Pet;
            Sprite newReportButton = Report;

            if (!Main.EnableCustomButton.Value || !Main.ProcessShapeshifts) goto EndOfSelectImg;

            switch (player.GetCustomRole())
            {
                case CustomRoles.Shifter:
                    newKillButton = CustomButton.Get("Swap");
                    break;
                case CustomRoles.Vulture:
                    newReportButton = CustomButton.Get("Eat");
                    break;
                case CustomRoles.Amnesiac:
                    if (Amnesiac.RememberMode.GetValue() == 0) newKillButton = CustomButton.Get("AmnesiacKill");
                    else newReportButton = CustomButton.Get("AmnesiacReport");
                    break;
                case CustomRoles.Undertaker:
                case CustomRoles.Glitch:
                    if (Main.PlayerStates[player.PlayerId].Role is not Glitch gc) break;
                    if (gc.KCDTimer > 0 && gc.HackCDTimer <= 0) newKillButton = CustomButton.Get("GlitchHack");
                    newSabotageButton = CustomButton.Get("GlitchMimic");
                    break;
                case CustomRoles.Jester:
                    newAbilityButton = CustomButton.Get("JesterVent");
                    break;
                case CustomRoles.ImperiusCurse:
                case CustomRoles.Sapper:
                case CustomRoles.Bomber:
                case CustomRoles.Nuker:
                    if (Options.UsePets.GetBool())
                        newPetButton = CustomButton.Get("Bomb");
                    else
                        newAbilityButton = CustomButton.Get("Bomb");
                    break;
                case CustomRoles.Agitater:
                    newKillButton = CustomButton.Get("Pass");
                    break;
                case CustomRoles.Arsonist:
                    newKillButton = CustomButton.Get("Douse");
                    if (player.IsDouseDone() || (Options.ArsonistCanIgniteAnytime.GetBool() && Utils.GetDousedPlayerCount(player.PlayerId).Item1 >= Options.ArsonistMinPlayersToIgnite.GetInt())) newVentButton = CustomButton.Get("Ignite");
                    break;
                case CustomRoles.Pyromaniac:
                    newKillButton = CustomButton.Get("Pyromaniac");
                    break;
                case CustomRoles.Mayor when Mayor.MayorHasPortableButton.GetBool():
                    if (Options.UsePets.GetBool())
                        newPetButton = CustomButton.Get("Button");
                    else
                        newAbilityButton = CustomButton.Get("Button");
                    break;
                case CustomRoles.Medic:
                    newKillButton = CustomButton.Get("Shield");
                    break;
                case CustomRoles.Vampire:
                    newKillButton = CustomButton.Get("Bite");
                    break;
                case CustomRoles.Veteran:
                    if (Options.UsePets.GetBool())
                        newPetButton = CustomButton.Get("Veteran");
                    else
                        newAbilityButton = CustomButton.Get("Veteran");
                    break;
                case CustomRoles.Romantic:
                    newKillButton = CustomButton.Get(!Romantic.HasPickedPartner ? "Romance" : "RomanticProtect");
                    break;
                case CustomRoles.VengefulRomantic:
                    newKillButton = CustomButton.Get("RomanticKill");
                    break;
                case CustomRoles.Sheriff:
                    newKillButton = CustomButton.Get("Kill");
                    break;
                case CustomRoles.Chameleon:
                    newAbilityButton = CustomButton.Get("invisible");
                    break;
                case CustomRoles.Escapee:
                    if (Options.UsePets.GetBool())
                        newPetButton = CustomButton.Get("abscond");
                    else
                        newAbilityButton = CustomButton.Get("abscond");
                    break;
                default:
                    if (player.GetCustomRole().UsesPetInsteadOfKill())
                    {
                        newPetButton = __instance.KillButton.graphic.sprite;
                    }

                    break;
            }

            if (player.GetCustomRole().UsesPetInsteadOfKill())
            {
                newPetButton = newKillButton;
            }


            EndOfSelectImg:

            __instance.KillButton.graphic.sprite = newKillButton;
            __instance.AbilityButton.graphic.sprite = newAbilityButton;
            __instance.ImpostorVentButton.graphic.sprite = newVentButton;
            __instance.SabotageButton.graphic.sprite = newSabotageButton;
            __instance.PetButton.graphic.sprite = newPetButton;
            __instance.ReportButton.graphic.sprite = newReportButton;

            __instance.KillButton.graphic.SetCooldownNormalizedUvs();
            __instance.AbilityButton.graphic.SetCooldownNormalizedUvs();
            __instance.ImpostorVentButton.graphic.SetCooldownNormalizedUvs();
            __instance.SabotageButton.graphic.SetCooldownNormalizedUvs();
            __instance.PetButton.graphic.SetCooldownNormalizedUvs();
            __instance.ReportButton.graphic.SetCooldownNormalizedUvs();
        }
        catch (Exception e)
        {
            if (Utils.TimeStamp - LastErrorTime > 10)
            {
                LastErrorTime = Utils.TimeStamp;
                Utils.ThrowException(e);
            }
        }
    }
}