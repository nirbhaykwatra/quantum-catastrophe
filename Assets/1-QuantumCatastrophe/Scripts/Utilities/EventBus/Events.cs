using QC.Systems.Entanglement;
using QC.Systems.Notifications;
using QC.Systems.Tutorials;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Marker interface for all event types. Implement this interface on custom event structs to create new event types.
    /// </summary>
    public interface IEvent { }
    
    // Concrete events
    public struct OnModeChanged : IEvent
    {
        public PlayerMode Mode;
    }

    public struct OnToggleEntanglement : IEvent { }

    public struct OnToggleSuperposition : IEvent
    {
        public bool Observed;
    }
    
    public struct OnClickEntangle : IEvent { }
    
    public struct OnTakeDamage : IEvent
    {
        public int Damage;
    }
    
    public struct OnHeal : IEvent
    {
        public int Amount;
    }
    
    public struct OnDeath : IEvent
    {
        public bool IsPlayer;
    }
    
    public struct OnEntanglementPairFormed : IEvent
    {
        public EntanglableComponent Source;
        public EntanglableComponent Target;
    }

    public struct OnEntanglementPairBroken : IEvent
    {
        public EntanglableComponent Source;
        public EntanglableComponent Target;
    }
    
    public struct OnRequestNotification : IEvent
    {
        public string Message;
        public NotificationType Type; // Info, Achievement, Warning, PickupConfirm...
        public Sprite Icon;
        public float Duration;
    }
    
    public struct OnDismissNotification : IEvent { }
    
    public struct OnRequestTutorialEvent : IEvent
    {
        public TutorialSequenceSO Sequence;
    }

    // Generic signal channel. Gameplay code (including TutorialSignalInteractBehavior)
    // publishes this; the tutorial controller checks it against the current step's
    // requiredEventName. Using one generic struct avoids needing a new event type
    // per tutorial trigger.
    public struct OnGameplaySignalEvent : IEvent
    {
        public string SignalName;
    }

    public struct OnTutorialModalOpened: IEvent { }

    public struct OnTutorialModalClosed : IEvent { }

    public struct OnTutorialCompleted : IEvent
    {
        public string TutorialId;
    }

    public struct OnZoomOut : IEvent { }
    
    public struct OnResetLevel : IEvent { }
    
}