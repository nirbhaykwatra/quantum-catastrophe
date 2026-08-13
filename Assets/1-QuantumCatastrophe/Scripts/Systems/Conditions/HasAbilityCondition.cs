using QC.Character;
using UnityEngine;

[System.Serializable]
public class AbilityCondition : ICondition
{
    [SerializeField] private Abilities requiredAbility;
    [TextArea]
    [SerializeField] private string SuccessMessage = "You have the required ability.";
    [TextArea]
    [SerializeField] private string failureMessage = "You need a specific ability to use this.";

    public bool IsConditionMet(GameObject interactor)
    {
        CharacterAbilities abilities = interactor.GetComponent<CharacterAbilities>();
        if (abilities == null) return false;

        return abilities.HasAbility(requiredAbility);
    }
    
    public void PostConditionCheck(GameObject interactor)
    {
        
    }

    public string GetFailureMessage() => failureMessage;
    public string GetSuccessMessage() => SuccessMessage;
}