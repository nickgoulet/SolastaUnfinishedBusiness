﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static ActionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class RangerLightBearer : AbstractSubclass
{
    internal const string Name = "RangerLightBearer";

    public RangerLightBearer()
    {
        // LEVEL 03

        // Lightbearer Magic

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetAutoTag("Ranger")
            .SetSpellcastingClass(CharacterClassDefinitions.Ranger)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Bless),
                BuildSpellGroup(5, BrandingSmite),
                BuildSpellGroup(9, SpellsContext.BlindingSmite),
                BuildSpellGroup(13, GuardianOfFaith),
                BuildSpellGroup(17, SpellsContext.BanishingSmite))
            .AddToDB();

        var powerLight = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Light")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerLight", Resources.PowerLight, 256, 128))
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(Light.EffectDescription)
            .AddCustomSubFeatures(new CustomBehaviorLight())
            .AddToDB();

        var featureSetLight = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Light")
            .SetGuiPresentationNoContent(true)
            .AddFeatureSet(powerLight)
            .AddToDB();

        // Blessed Warrior

        var conditionBlessedWarrior = ConditionDefinitionBuilder
            .Create($"Condition{Name}BlessedWarrior")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionMarkedByBrandingSmite)
            .SetConditionType(ConditionType.Detrimental)
            .CopyParticleReferences(ConditionDefinitions.ConditionMarkedByHunter)
            .AddToDB();

        var powerBlessedWarrior = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}BlessedWarrior")
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerBlessedWarrior", Resources.PowerBlessedWarrior, 256, 128))
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionBlessedWarrior, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(new PhysicalAttackBeforeHitConfirmedOnEnemyBlessedWarrior(conditionBlessedWarrior))
            .AddToDB();

        // Lifebringer

        var attributeModifierLifeBringerBase = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}LifeBringerBase")
            .SetGuiPresentationNoContent(true)
            .SetModifier(
                AttributeModifierOperation.Set,
                AttributeDefinitions.HealingPool)
            .AddToDB();

        var attributeModifierLifeBringerAdditive = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}LifeBringerAdditive")
            .SetGuiPresentationNoContent(true)
            .SetModifier(
                AttributeModifierOperation.Additive,
                AttributeDefinitions.HealingPool, 1)
            .AddToDB();

        var powerLifeBringer = FeatureDefinitionPowerBuilder
            .Create(FeatureDefinitionPowers.PowerPaladinLayOnHands, $"Power{Name}LifeBringer")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerLifeBringer", Resources.PowerLifeBringer, 256, 128))
            .SetUsesFixed(ActivationTime.Action, RechargeRate.HealingPool, 0)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(FeatureDefinitionPowers.PowerPaladinLayOnHands.EffectDescription)
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.Individuals)
                    .Build())
            .AddToDB();

        // LEVEL 07

        // Blessed Glow

        var conditionBlindedByBlessedGlow = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionBlinded, "ConditionBlindedByBlessedGlow")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetParentCondition(ConditionDefinitions.ConditionBlinded)
            .SetFeatures()
            .AddToDB();

        conditionBlindedByBlessedGlow.GuiPresentation.description = "Rules/&ConditionBlindedDescription";

        var powerBlessedGlow = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}BlessedGlow")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Enemy, RangeType.Self, 0, TargetType.Sphere, 4)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Constitution,
                        false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionBlindedByBlessedGlow,
                                ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.EndOfTurn, true)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        powerBlessedGlow.EffectDescription.savingThrowAffinitiesByFamily =
        [
            new SaveAffinityByFamilyDescription { advantageType = AdvantageType.Disadvantage, family = "Fiend" },
            new SaveAffinityByFamilyDescription { advantageType = AdvantageType.Disadvantage, family = "Undead" }
        ];

        var powerLightEnhanced = FeatureDefinitionPowerBuilder
            .Create(powerLight, $"Power{Name}LightEnhanced")
            .SetOverriddenPower(powerLight)
            .AddCustomSubFeatures(new PowerOrSpellFinishedByMeBlessedGlow(powerBlessedGlow))
            .AddToDB();

        var featureSetBlessedGlow = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}BlessedGlow")
            .SetGuiPresentation($"Power{Name}BlessedGlow", Category.Feature)
            .AddFeatureSet(powerLightEnhanced, powerBlessedGlow)
            .AddToDB();

        // LEVEL 11

        // Angelic Form

        var conditionAngelicForm = ConditionDefinitionBuilder
            .Create($"Condition{Name}AngelicForm")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionShine)
            .AddFeatures(
                FeatureDefinitionAttackModifierBuilder
                    .Create($"AttackModifier{Name}AngelicForm")
                    .SetGuiPresentation($"Condition{Name}AngelicForm", Category.Condition, Gui.NoLocalization)
                    // cannot use SetMagicalWeapon as it doesn't trigger with flurry of blows
                    .AddCustomSubFeatures(new ModifyAttackActionModifierAngelicForm())
                    .AddToDB())
            .AddToDB();

        var powerAngelicFormSprout = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicFormSprout")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerAngelicFormSprout", Resources.PowerAngelicFormSprout, 256, 128))
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionAngelicForm,
                                ConditionForm.ConditionOperation.Add)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                new PowerOrSpellFinishedByMeAngelicForm(),
                new ValidatorsValidatePowerUse(ValidatorsCharacter.HasNoneOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var powerAngelicFormDismiss = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicFormDismiss")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerAngelicFormDismiss", Resources.PowerAngelicFormDismiss, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionAngelicForm,
                                ConditionForm.ConditionOperation.Remove)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Remove)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                new ValidatorsValidatePowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var featureSetAngelicForm = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}AngelicForm")
            .SetGuiPresentation($"Power{Name}AngelicFormSprout", Category.Feature)
            .AddFeatureSet(powerAngelicFormSprout, powerAngelicFormDismiss)
            .AddToDB();

        // LEVEL 15

        // Warding Light

        var actionAffinityWardingLight = FeatureDefinitionActionAffinityBuilder
            .Create($"ActionAffinity{Name}WardingLight")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions(Id.BlockAttack)
            .AddToDB();

        var featureWardingLight = FeatureDefinitionBuilder
            .Create($"Feature{Name}WardingLight")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(new PhysicalAttackInitiatedOnMeOrAllyWardingLight())
            .AddToDB();

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.RangerLightBearer, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                featureSetLight,
                powerBlessedWarrior,
                attributeModifierLifeBringerBase,
                attributeModifierLifeBringerAdditive,
                powerLifeBringer)
            .AddFeaturesAtLevel(7,
                featureSetBlessedGlow)
            .AddFeaturesAtLevel(11,
                featureSetAngelicForm)
            .AddFeaturesAtLevel(15,
                actionAffinityWardingLight,
                featureWardingLight)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Ranger;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRangerArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Light
    //

    private sealed class CustomBehaviorLight : ICustomLevelUpLogic, IPowerOrSpellFinishedByMe
    {
        public void ApplyFeature(RulesetCharacterHero hero, string tag)
        {
            var repertoire = hero.SpellRepertoires.FirstOrDefault(x =>
                x.SpellCastingClass == CharacterClassDefinitions.Ranger);

            repertoire?.knownCantrips.Add(Light);
        }

        public void RemoveFeature(RulesetCharacterHero hero, string tag)
        {
            // empty
        }

        public IEnumerator OnPowerOrSpellFinishedByMe(CharacterActionMagicEffect action, BaseDefinition baseDefinition)
        {
            var actingCharacter = action.ActingCharacter;
            var rulesetCharacter = actingCharacter.RulesetCharacter;
            var rulesetRepertoire = rulesetCharacter.SpellRepertoires.FirstOrDefault(x =>
                x.SpellCastingClass == CharacterClassDefinitions.Ranger);
            var effectSpell = ServiceRepository.GetService<IRulesetImplementationService>()
                .InstantiateEffectSpell(rulesetCharacter, rulesetRepertoire, Light, 0, false);

            effectSpell.TrackedLightSourceGuids.AddRange(action.ActionParams.RulesetEffect.TrackedLightSourceGuids);
            action.ActionParams.RulesetEffect.TrackedLightSourceGuids.Clear();
            rulesetCharacter.SpellsCastByMe.Add(effectSpell);

            yield break;
        }
    }

    //
    // Blessed Warrior
    //

    private sealed class PhysicalAttackBeforeHitConfirmedOnEnemyBlessedWarrior(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDefinition) : IPhysicalAttackBeforeHitConfirmedOnEnemy
    {
        public IEnumerator OnPhysicalAttackBeforeHitConfirmedOnEnemy(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier actionModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            if (attackMode == null)
            {
                yield break;
            }

            var rulesetDefender = defender.RulesetActor;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                !rulesetDefender.HasConditionOfCategoryAndType(AttributeDefinitions.TagEffect,
                    conditionDefinition.Name))
            {
                yield break;
            }

            // change all damage types to radiant
            foreach (var damageForm in actualEffectForms
                         .Where(x => x.FormType == EffectForm.EffectFormType.Damage))
            {
                damageForm.DamageForm.damageType = DamageTypeRadiant;
            }

            // add additional radiant damage form
            var diceNumber = attacker.RulesetCharacter.GetClassLevel(CharacterClassDefinitions.Ranger) < 11 ? 1 : 2;
            var pos = actualEffectForms.FindIndex(x => x.FormType == EffectForm.EffectFormType.Damage);

            if (pos >= 0)
            {
                actualEffectForms.Insert(
                    pos + 1,
                    EffectFormBuilder.DamageForm(DamageTypeRadiant, diceNumber, DieType.D8));
            }

            if (rulesetDefender.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect,
                    conditionDefinition.Name,
                    out var activeCondition))
            {
                rulesetDefender.RemoveCondition(activeCondition);
            }
        }
    }

    //
    // Blessed Glow
    //

    private sealed class PowerOrSpellFinishedByMeBlessedGlow(FeatureDefinitionPower featureDefinitionPower)
        : IPowerOrSpellFinishedByMe
    {
        public IEnumerator OnPowerOrSpellFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            if (ServiceRepository.GetService<IGameLocationBattleService>() is not GameLocationBattleManager
                {
                    IsBattleInProgress: true
                } battleManager)
            {
                yield break;
            }

            var attacker = action.ActingCharacter;
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.GetRemainingPowerCharges(featureDefinitionPower) <= 0)
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var implementationManager =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var usablePower = PowerProvider.Get(featureDefinitionPower, rulesetAttacker);
            var targets = battleManager.Battle
                .GetContenders(attacker, withinRange: 5);
            var actionModifiers = new List<ActionModifier>();

            for (var i = 0; i < targets.Count; i++)
            {
                actionModifiers.Add(new ActionModifier());
            }

            var actionParams = new CharacterActionParams(attacker, Id.PowerNoCost)
            {
                StringParameter = "BlessedGlow",
                ActionModifiers = actionModifiers,
                RulesetEffect = implementationManager
                    .MyInstantiateEffectPower(rulesetAttacker, usablePower, false),
                UsablePower = usablePower,
                targetCharacters = targets
            };
            var count = actionService.PendingReactionRequestGroups.Count;

            actionService.ReactToUsePower(actionParams, "UsePower", attacker);

            yield return battleManager.WaitForReactions(attacker, actionService, count);
        }
    }

    //
    // Angelic Form
    //

    private sealed class PowerOrSpellFinishedByMeAngelicForm : IPowerOrSpellFinishedByMe
    {
        public IEnumerator OnPowerOrSpellFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            var rulesetCharacter = action.ActingCharacter.RulesetCharacter;
            var classLevel = rulesetCharacter.GetClassLevel(CharacterClassDefinitions.Ranger);

            rulesetCharacter.ReceiveTemporaryHitPoints(
                classLevel, DurationType.UntilAnyRest, 0, TurnOccurenceType.StartOfTurn, rulesetCharacter.Guid);

            yield break;
        }
    }

    private sealed class ModifyAttackActionModifierAngelicForm : IModifyAttackActionModifier
    {
        public void OnAttackComputeModifier(
            RulesetCharacter myself,
            RulesetCharacter defender,
            BattleDefinitions.AttackProximity attackProximity,
            RulesetAttackMode attackMode,
            string effectName,
            ref ActionModifier attackModifier)
        {
            attackMode.AttackTags.TryAdd(TagsDefinitions.MagicalWeapon);
        }
    }

    //
    // Warding Light
    //

    private sealed class PhysicalAttackInitiatedOnMeOrAllyWardingLight : IPhysicalAttackInitiatedOnMeOrAlly
    {
        public IEnumerator OnPhysicalAttackInitiatedOnMeOrAlly(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            if (battleManager is not { IsBattleInProgress: true })
            {
                yield break;
            }

            using var onPhysicalAttackInitiatedOnMeOrAlly = battleManager.Battle.GetContenders(attacker, withinRange: 6)
                .Where(opposingContender =>
                    opposingContender.CanReact() &&
                    opposingContender.CanPerceiveTarget(attacker) &&
                    opposingContender.CanPerceiveTarget(defender) &&
                    opposingContender.GetActionStatus(
                        Id.BlockAttack, ActionScope.Battle, ActionStatus.Available) == ActionStatus.Available)
                .Select(opposingContender => battleManager.PrepareAndReact(
                    opposingContender, attacker, attacker, Id.BlockAttack, attackModifier,
                    additionalTargetCharacter: defender))
                .GetEnumerator();

            yield return onPhysicalAttackInitiatedOnMeOrAlly;
        }
    }
}
