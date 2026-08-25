using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class BaseCondition
{
    public string ConditionName = "Custom Condition";
    
    [TextArea]
    public string SuccessMessage = "Condition met.";
    
    [TextArea]
    public string FailureMessage = "Condition not met.";
    
    public virtual bool IsConditionMet(in InteractionContext context)
    {
        return false;
    }
    
    public virtual void PostConditionCheck(in InteractionContext context) { }
}
