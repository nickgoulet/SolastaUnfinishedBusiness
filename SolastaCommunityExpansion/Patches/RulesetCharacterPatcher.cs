﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaCommunityExpansion.CustomInterfaces;
using SolastaCommunityExpansion.Feats;
using SolastaCommunityExpansion.Models;

namespace SolastaCommunityExpansion.Patches;

internal static class RulesetCharacterPatcher
{
    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniquePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class TerminateMatchingUniquePower_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, FeatureDefinitionPower powerDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniquePower(__instance, powerDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniqueSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class TerminateMatchingUniqueSpell_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, SpellDefinition spellDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniqueSpell(__instance, spellDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionAdded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class OnConditionAdded_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is applied 
            activeCondition.ConditionDefinition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.ApplyFeature(__instance));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionRemoved")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class OnConditionRemoved_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is removed 
            activeCondition.ConditionDefinition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.RemoveFeature(__instance));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentSomaticValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsComponentSomaticValid_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition spellDefinition, ref string failure)
        {
            //PATCH: Allows valid Somatic component if specific material component is held in main hand or off hand slots
            // allows casting somatic spells with full hands if one of the hands holds metarial component for the spell
            if (__result || spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spellDefinition.SpecificMaterialComponentTag;
            var inventorySlotsByName = __instance.CharacterInventory.InventorySlotsByName;
            var mainHand = inventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
            var offHand = inventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;
            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();

            mainHand?.FillTags(tagsMap, __instance, true);
            offHand?.FillTags(tagsMap, __instance, true);

            if (!tagsMap.ContainsKey(materialTag))
            {
                return;
            }

            __result = true;
            failure = string.Empty;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentMaterialValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsComponentMaterialValid_Patch
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition spellDefinition, ref string failure)
        {
            //PATCH: Allows spells to satisfy specific material components by actual active tags on an item that are not directly defined in ItemDefinition (like "Melee")
            //Used mostly for melee cantrips requiring melee weapon to cast
            if (__result || spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spellDefinition.SpecificMaterialComponentTag;
            var requiredCost = spellDefinition.SpecificMaterialComponentCostGp;

            List<RulesetItem> items = new();
            __instance.CharacterInventory.EnumerateAllItems(items);
            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();
            foreach (var rulesetItem in items)
            {
                tagsMap.Clear();
                rulesetItem.FillTags(tagsMap, __instance, true);
                var itemItemDefinition = rulesetItem.ItemDefinition;
                var costInGold = EquipmentDefinitions.GetApproximateCostInGold(itemItemDefinition.Costs);

                if (!tagsMap.ContainsKey(materialTag) || costInGold < requiredCost)
                {
                    continue;
                }

                __result = true;
                failure = string.Empty;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsValidReadyCantrip")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacter_IsValidReadyCantrip
    {
        internal static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition cantrip)
        {
            //PATCH: Modifies validity of ready cantrip action to include attack cantrips even if they don't have damage forms
            //makes melee cantrips valid for ready action
            if (__result)
            {
                return;
            }

            var effect = CustomFeaturesContext.ModifySpellEffect(cantrip, __instance);
            var hasDamage = effect.HasFormOfType(EffectForm.EffectFormType.Damage);
            var hasAttack = cantrip.HasSubFeatureOfType<IPerformAttackAfterMagicEffectUse>();
            var notGadgets = effect.TargetFilteringMethod != RuleDefinitions.TargetFilteringMethod.GadgetOnly;
            var componentsValid = __instance.AreSpellComponentsValid(cantrip);

            __result = (hasDamage || hasAttack) && notGadgets && componentsValid;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsSubjectToAttackOfOpportunity")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class IsSubjectToAttackOfOpportunity_Patch
    {
        // ReSharper disable once RedundantAssignment
        internal static void Postfix(RulesetCharacter __instance, ref bool __result, RulesetCharacter attacker)
        {
            //PATCH: allows custom exceptions for attack of opportunity triggering
            //Mostly for Sentinel feat
            __result = AttacksOfOpportunity.IsSubjectToAttackOfOpportunity(__instance, attacker, __result);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "ComputeSaveDC")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ComputeSaveDC_Patch
    {
        // ReSharper disable once RedundantAssignment
        internal static void Postfix(RulesetCharacter __instance, ref int __result)
        {
            //PATCH: support for `IIncreaseSpellDC`
            //Adds extra modifiers to spell DC
            
            var features = __instance.GetSubFeaturesByType<IIncreaseSpellDC>();
            __result += features.Where(feature => feature != null).Sum(feature => feature.GetSpellModifier(__instance));
        }
    }
    
    
}
