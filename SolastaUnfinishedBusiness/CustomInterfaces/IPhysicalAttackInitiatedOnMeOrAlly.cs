﻿using System.Collections;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IPhysicalAttackInitiatedOnMeOrAlly
{
    [UsedImplicitly]
    IEnumerator OnAttackInitiated(
        GameLocationBattleManager __instance,
        CharacterAction action,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        ActionModifier attackModifier,
        RulesetAttackMode attackerAttackMode);
}
