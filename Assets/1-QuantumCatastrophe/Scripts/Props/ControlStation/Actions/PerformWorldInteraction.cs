using System;
using UnityEngine.Events;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class PerformWorldInteraction : BaseControlStationAction
    {
        public UnityEvent OnWorldInteract;
        
        public override void Execute(in InteractionContext context)
        {
            OnWorldInteract?.Invoke();
        }
    }
}