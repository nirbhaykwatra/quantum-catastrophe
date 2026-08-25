using QC.Character;
using UnityEngine;

[System.Serializable]
public class AbilityCondition : BaseCondition
{
    [SerializeField] private Abilities requiredAbility;

    public override bool IsConditionMet(in InteractionContext context)
    {
        CharacterAbilities abilities = context.Interactor.GetComponent<CharacterAbilities>();
        if (abilities == null) return false;

        return abilities.HasAbility(requiredAbility);
    }
}