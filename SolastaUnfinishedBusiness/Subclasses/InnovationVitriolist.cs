using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Classes;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class InnovationVitriolist : AbstractSubclass
{
    private const string Name = "InnovationVitriolist";

    public InnovationVitriolist()
    {
        // LEVEL 03

        // Auto Prepared Spells

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(InventorClass.Class)
            .SetAutoTag("InnovationVitriolist")
            .AddPreparedSpellGroup(3, SpellsContext.CausticZap, Shield)
            .AddPreparedSpellGroup(5, AcidArrow, Blindness)
            .AddPreparedSpellGroup(9, ProtectionFromEnergy, StinkingCloud)
            .AddPreparedSpellGroup(13, Blight, Stoneskin)
            .AddPreparedSpellGroup(17, CloudKill, Contagion)
            .AddToDB();

        // Vitriolic Mixtures

        var powerMixture = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Mixture")
            .SetGuiPresentation(Category.Feature, PowerPactChainSprite)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest, 1, 0)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .Build())
            .AddToDB();

        powerMixture.AddCustomSubFeatures(
            HasModifiedUses.Marker,
            new ModifyPowerPoolAmount
            {
                PowerPool = powerMixture,
                Type = PowerPoolBonusCalculationType.AttributeModifier,
                Attribute = AttributeDefinitions.Intelligence
            },
            new ModifyPowerPoolAmount
            {
                PowerPool = powerMixture,
                Type = PowerPoolBonusCalculationType.Attribute,
                Attribute = AttributeDefinitions.ProficiencyBonus
            });

        // Corrosion

        var conditionCorroded = ConditionDefinitionBuilder
            .Create($"Condition{Name}Corroded")
            .SetGuiPresentation(Category.Condition, Gui.NoLocalization, ConditionDefinitions.ConditionHeatMetal)
            .SetConditionType(ConditionType.Detrimental)
            .AddFeatures(
                FeatureDefinitionAttributeModifierBuilder
                    .Create($"AttributeModifier{Name}Corroded")
                    .SetGuiPresentation($"Condition{Name}Corroded", Category.Condition)
                    .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, -2)
                    .AddToDB())
            .AddToDB();

        var powerCorrosion = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}Corrosion")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerMixture)
            .SetUseSpellAttack()
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .RollSaveOnlyIfRelevantForms()
                    .SetParticleEffectParameters(AcidSplash)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeAcid, 2, DieType.D8)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                            .Build(),
                        EffectFormBuilder.ConditionForm(conditionCorroded))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        // Misery

        var conditionMiserable = ConditionDefinitionBuilder
            .Create($"Condition{Name}Miserable")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionAcidArrowed)
            .SetConditionType(ConditionType.Detrimental)
            .SetRecurrentEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeAcid, 2, DieType.D4)
                    .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                    .SetCreatedBy()
                    .Build())
            .AddToDB();

        var powerMisery = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}Misery")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerMixture)
            .SetUseSpellAttack()
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .RollSaveOnlyIfRelevantForms()
                    .SetParticleEffectParameters(AcidArrow)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeAcid, 2, DieType.D8)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                            .Build(),
                        EffectFormBuilder.ConditionForm(conditionMiserable))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        // Affliction

        var powerAffliction = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}Affliction")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerMixture)
            .SetUseSpellAttack()
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .RollSaveOnlyIfRelevantForms()
                    .SetParticleEffectParameters(AcidSplash)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeAcid, 2, DieType.D4)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypePoison, 2, DieType.D4)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                            .Build(),
                        EffectFormBuilder.ConditionForm(ConditionDefinitions.ConditionPoisoned))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        // Viscosity

        var powerViscosity = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}Viscosity")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerMixture)
            .SetUseSpellAttack()
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .RollSaveOnlyIfRelevantForms()
                    .SetParticleEffectParameters(PowerDomainOblivionMarkOfFate)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeAcid, 2, DieType.D8)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 0, 20, (7, 1), (14, 2), (18, 3))
                            .Build(),
                        EffectFormBuilder.ConditionForm(ConditionDefinitions.ConditionHindered))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        // Mixture

        var mixturePowers = new FeatureDefinition[] { powerCorrosion, powerMisery, powerAffliction, powerViscosity };

        var featureSetMixture = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Mixture")
            .SetGuiPresentation(Category.Feature)
            .SetFeatureSet(powerMixture)
            .AddFeatureSet(mixturePowers)
            .AddToDB();

        PowerBundle.RegisterPowerBundle(powerMixture, true, mixturePowers.OfType<FeatureDefinitionPower>());

        // LEVEL 05

        // Vitriolic Infusion

        // kept name for backward compatibility
        var featureDamageInfusion = FeatureDefinitionBuilder
            .Create($"AdditionalDamage{Name}Infusion")
            .SetGuiPresentationNoContent(true)
            .AddCustomSubFeatures(new CustomBehaviorVitriolicInfusion())
            .AddToDB();

        var featureSetVitriolicInfusion = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Infusion")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(featureDamageInfusion, DamageAffinityAcidResistance)
            .AddToDB();

        // LEVEL 09

        // Vitriolic Arsenal - Refund Mixture

        var powerRefundMixture = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}RefundMixture")
            .SetGuiPresentation(Category.Feature, PowerDomainInsightForeknowledge)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .Build())
            .AddCustomSubFeatures(new CustomBehaviorRefundMixture(powerMixture))
            .AddToDB();

        // Vitriolic Arsenal - Prevent Reactions

        var conditionArsenal = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionShocked, $"Condition{Name}Arsenal")
            .SetSpecialDuration(DurationType.Round, 0, TurnOccurenceType.StartOfTurn)
            .SetFeatures(
                FeatureDefinitionActionAffinityBuilder
                    .Create($"ActionAffinity{Name}Arsenal")
                    .SetGuiPresentationNoContent(true)
                    .SetAllowedActionTypes(reaction: false)
                    .AddToDB())
            .AddToDB();

        // Vitriolic Arsenal - Bypass Resistance and Change Immunity to Resistance

        var featureArsenal = FeatureDefinitionBuilder
            .Create($"Feature{Name}Arsenal")
            .SetGuiPresentation($"FeatureSet{Name}Arsenal", Category.Feature)
            .AddCustomSubFeatures(new ModifyDamageAffinityArsenal())
            .AddToDB();

        // Vitriolic Arsenal

        var featureSetArsenal = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Arsenal")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(powerRefundMixture, featureArsenal)
            .AddToDB();

        // LEVEL 15

        // Vitriolic Paragon

        var featureParagon = FeatureDefinitionBuilder
            .Create($"Feature{Name}Paragon")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        // MAIN

        powerMixture.AddCustomSubFeatures(
            new ModifyEffectDescriptionMixture(
                conditionArsenal, ConditionDefinitions.ConditionIncapacitated, mixturePowers));

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, TraditionShockArcanist)
            .AddFeaturesAtLevel(3, autoPreparedSpells, featureSetMixture)
            .AddFeaturesAtLevel(5, featureSetVitriolicInfusion)
            .AddFeaturesAtLevel(9, featureSetArsenal)
            .AddFeaturesAtLevel(15, featureParagon)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }
    internal override CharacterClassDefinition Klass => InventorClass.Class;
    internal override FeatureDefinitionSubclassChoice SubclassChoice => InventorClass.SubclassChoice;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Mixtures - add shocked at 9 and paralyzed at 15
    //

    private sealed class ModifyEffectDescriptionMixture(
        ConditionDefinition conditionArsenal,
        ConditionDefinition conditionParagon,
        params FeatureDefinition[] mixturePowers) : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return character.GetClassLevel(InventorClass.Class) >= 5;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            var levels = character.GetClassLevel(InventorClass.Class);

            // Arsenal - add shocked at 9
            if (levels >= 9 && mixturePowers.Contains(definition))
            {
                effectDescription.EffectForms.Add(EffectFormBuilder.ConditionForm(conditionArsenal));
            }

            // Paragon - add paralyzed at 15
            if (levels >= 15 && mixturePowers.Contains(definition))
            {
                effectDescription.EffectForms.Add(
                    EffectFormBuilder
                        .Create()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(conditionParagon, ConditionForm.ConditionOperation.Add)
                        .Build());
            }

            return effectDescription;
        }
    }

    //
    // Refund Mixture
    //

    private sealed class CustomBehaviorRefundMixture(FeatureDefinitionPower powerMixture)
        : IPowerOrSpellFinishedByMe
    {
        public IEnumerator OnPowerOrSpellFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            var battleManager = ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (!battleManager)
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var actingCharacter = action.ActingCharacter;
            var rulesetCharacter = actingCharacter.RulesetCharacter;
            var spellRepertoire = rulesetCharacter.GetClassSpellRepertoire(InventorClass.Class);
            var slotLevel = spellRepertoire!.GetLowestAvailableSlotLevel();
            var reactionParams = new CharacterActionParams(actingCharacter, ActionDefinitions.Id.SpendSpellSlot)
            {
                IntParameter = slotLevel, StringParameter = "RefundMixture", SpellRepertoire = spellRepertoire
            };
            var count = actionService.PendingReactionRequestGroups.Count;

            actionService.ReactToSpendSpellSlot(reactionParams);

            yield return battleManager.WaitForReactions(actingCharacter, actionService, count);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            var slotUsed = reactionParams.IntParameter;
            var usablePower = PowerProvider.Get(powerMixture, rulesetCharacter);

            rulesetCharacter.UpdateUsageForPowerPool(-slotUsed, usablePower);
            spellRepertoire.SpendSpellSlot(slotUsed);
        }
    }

    //
    // Arsenal - bypass acid resistance / change acid immunity to acid resistance
    //

    private sealed class ModifyDamageAffinityArsenal : IModifyDamageAffinity
    {
        public void ModifyDamageAffinity(
            RulesetActor defender,
            RulesetActor attacker,
            List<FeatureDefinition> features)
        {
            features.RemoveAll(x =>
                x is IDamageAffinityProvider
                {
                    DamageAffinityType: DamageAffinityType.Resistance, DamageType: DamageTypeAcid
                });

            var immunityCount = features.RemoveAll(x =>
                x is IDamageAffinityProvider
                {
                    DamageAffinityType: DamageAffinityType.Immunity, DamageType: DamageTypeAcid
                });

            if (immunityCount > 0)
            {
                features.Add(DamageAffinityAcidResistance);
            }
        }
    }

    //
    // Vitriolic Infusion
    //

    private sealed class CustomBehaviorVitriolicInfusion
        : IMagicEffectInitiatedByMe, IPhysicalAttackInitiatedByMe, IMagicEffectFinishedByMe, IPhysicalAttackFinishedByMe
    {
        private readonly HashSet<GameLocationCharacter> _isValid = [];

        public IEnumerator OnMagicEffectFinishedByMe(
            CharacterActionMagicEffect action,
            GameLocationCharacter attacker,
            List<GameLocationCharacter> targets)
        {
            foreach (var target in targets)
            {
                target.RulesetActor.DamageReceived -= DamageReceivedHandler;

                if (_isValid.Contains(target))
                {
                    InflictDamage(attacker, target);
                }
            }

            _isValid.Clear();

            yield break;
        }

        public IEnumerator OnMagicEffectInitiatedByMe(
            CharacterActionMagicEffect action,
            RulesetEffect activeEffect,
            GameLocationCharacter attacker,
            List<GameLocationCharacter> targets)
        {
            foreach (var target in targets)
            {
                target.RulesetActor.DamageReceived += DamageReceivedHandler;
            }

            _isValid.Clear();

            yield break;
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            defender.RulesetActor.DamageReceived += DamageReceivedHandler;

            if (_isValid.Contains(defender))
            {
                InflictDamage(attacker, defender);
            }

            _isValid.Clear();

            yield break;
        }

        public IEnumerator OnPhysicalAttackInitiatedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            defender.RulesetActor.DamageReceived += DamageReceivedHandler;

            _isValid.Clear();

            yield break;
        }

        private void DamageReceivedHandler(
            RulesetActor rulesetDefender,
            int damage,
            string receivedDamageType,
            ulong sourceGuid,
            RollInfo rollInfo)
        {
            if (receivedDamageType != DamageTypeAcid)
            {
                return;
            }

            var glc = GameLocationCharacter.GetFromActor(rulesetDefender);

            if (glc != null)
            {
                _isValid.Add(glc);
            }
        }

        private static void InflictDamage(
            GameLocationCharacter attacker,
            GameLocationCharacter defender)
        {
            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetActor;
            var pb = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            var rolls = new List<int>();
            var damageForm = new DamageForm
            {
                DamageType = DamageTypeAcid, DieType = DieType.D20, DiceNumber = 0, BonusDamage = pb
            };
            var damageRoll = rulesetAttacker.RollDamage(damageForm, 0, false, 0, 0, 1, false, false, false, rolls);

            var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
            {
                sourceCharacter = rulesetAttacker,
                targetCharacter = rulesetDefender,
                position = defender.LocationPosition
            };

            rulesetAttacker.LogCharacterActivatesAbility(string.Empty, "Feedback/&AdditionalDamageInfusionLine");
            EffectHelpers.StartVisualEffect(attacker, defender, AcidSplash);
            RulesetActor.InflictDamage(
                damageRoll,
                damageForm,
                damageForm.DamageType,
                applyFormsParams,
                rulesetDefender,
                false,
                rulesetAttacker.Guid,
                false,
                [],
                new RollInfo(damageForm.DieType, rolls, damageForm.BonusDamage),
                false,
                out _);
        }
    }
}
