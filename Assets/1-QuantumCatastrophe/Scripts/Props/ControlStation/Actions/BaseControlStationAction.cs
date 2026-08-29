using System;
using System.Collections.Generic;
using QC.Systems.Notifications;
using QC.Utilities.EventBusSystem;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public abstract class BaseControlStationAction
    {
        public string ActionName;
        public string NotificationText;
        public Sprite NotificationIcon;
        public NotificationType NotificationType;
        public float NotificationDuration;
        public bool DebugConditions;
        
        [SerializeReference] 
        private List<BaseCondition> Conditions = new();

        public virtual void Execute(in InteractionContext context, UIEventBus eventBus) { }
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