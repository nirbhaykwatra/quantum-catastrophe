using System;
using QC.Utilities.EventBusSystem;
using UnityEngine.Events;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class PerformWorldInteraction : BaseControlStationAction
    {
        public UnityEvent OnWorldInteract;
        
        public override void Execute(in InteractionContext context, UIEventBus eventBus)
        {
            OnWorldInteract?.Invoke();
        }
    }
}