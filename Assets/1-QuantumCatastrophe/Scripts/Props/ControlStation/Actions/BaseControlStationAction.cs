using System;
using System.Collections.Generic;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public abstract class BaseControlStationAction
    {
        public string ActionName;
        public bool DebugConditions;
        
        [SerializeReference] 
        private List<BaseCondition> Conditions = new();

        public virtual void Execute(in InteractionContext context) { }
        public bool CheckConditions(in InteractionContext context)
        {
            foreach (BaseCondition condition in Conditions)
            {
                if (!condition.IsConditionMet(context))
                {
                    if (DebugConditions) ShowMessage(condition.FailureMessage);
                    return false;
                }
                if (DebugConditions) ShowMessage(condition.SuccessMessage);
                condition.PostConditionCheck(context);
            }
            return true;
        }
        
        private void ShowMessage(string message)
        {
            // NotificationManager.Instance.RequestModal(message);
            Debug.Log($"Condition: {message}");
        }
    }
}