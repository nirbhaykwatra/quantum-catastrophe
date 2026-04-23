using QC.Systems.Entanglement;
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
    
}