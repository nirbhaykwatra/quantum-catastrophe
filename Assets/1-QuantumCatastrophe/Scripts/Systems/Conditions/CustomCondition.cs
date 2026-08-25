using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomCondition : BaseCondition
{
    [SerializeField] 
    private UnityEvent<GameObject> _conditionCheck;
    private bool _lastCheckResult = false;

    public override bool IsConditionMet(in InteractionContext context)
    {
        _conditionCheck?.Invoke(context.Interactor);
        return _lastCheckResult;
    }

    public void SetResult(bool result)
    {
        _lastCheckResult = result;
    }
}