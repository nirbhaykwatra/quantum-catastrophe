namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Marker interface for all event types. Implement this interface on custom event structs to create new event types.
    /// </summary>
    public interface IEvent { }
    
    // Concrete events
    public struct ModeChangeEvent : IEvent
    {
        public PlayerMode Mode;
    }
}