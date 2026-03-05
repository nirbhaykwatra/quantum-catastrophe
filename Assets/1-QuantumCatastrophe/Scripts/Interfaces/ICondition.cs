using UnityEngine;

public interface ICondition
{
    bool IsConditionMet(GameObject interactor);
    void PostConditionCheck(GameObject interactor);
    string GetFailureMessage();
    string GetSuccessMessage();
}
