namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Marker interface for all event types. Implement this interface on custom event structs to create new event types.
    /// </summary>
    public interface IEvent { }
    
    // Concrete events
    public struct OnModeChange : IEvent
    {
        public PlayerMode Mode;
    }
    
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
    
}