﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class GuiSpellDefinitionPatcher
{
    [HarmonyPatch(typeof(GuiSpellDefinition), "EnumerateTags")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class EnumerateTags_Patch
    {
        public static void Postfix(GuiSpellDefinition __instance)
        {
            //PATCH: adds `Unfinished Business` tag to all CE spells
            CeContentPackContext.AddCeSpellTag(__instance.SpellDefinition, __instance.TagsMap);
        }
    }

    [HarmonyPatch(typeof(GuiSpellDefinition), "EffectDescription", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class EffectDescription_Getter_Patch
    {
        public static void Postfix(GuiSpellDefinition __instance, ref EffectDescription __result)
        {
            //PATCH: support for ICustomMagicEffectBasedOnCaster allowing to pick spell effect for GUI depending on caster properties
            __result = PowerBundle.ModifyMagicEffectGui(__result, __instance.SpellDefinition);
        }
    }
}
