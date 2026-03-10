namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Marker interface for all event types. Implement this interface on custom event structs to create new event types.
    /// </summary>
    public interface IEvent { }
    
    // Concrete events
    public struct DayTimeChangeEvent : IEvent
    {
        public bool IsDay;
    }
    
    public struct NeedsChangeEvent : IEvent
    {
        public float Fatigue;
    }

    public struct TextUIEvent : IEvent
    {
        public string For;
        public string Text;
    }
}