using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomCondition : ICondition
{
    [SerializeField] private string conditionName = "Custom Condition";
    [TextArea]
    [SerializeField] private string SuccessMessage = "Condition met.";
    [TextArea]
    [SerializeField] private string failureMessage = "Condition not met.";
    [SerializeField] private UnityEvent<GameObject> conditionCheck;
    
    private bool lastCheckResult = false;

    public bool IsConditionMet(GameObject interactor)
    {
        conditionCheck?.Invoke(interactor);
        return lastCheckResult;
    }

    public void SetResult(bool result)
    {
        lastCheckResult = result;
    }
    
    public void PostConditionCheck(GameObject interactor)
    {
        
    }

    public string GetFailureMessage() => failureMessage;
    public string GetSuccessMessage() => SuccessMessage;
}