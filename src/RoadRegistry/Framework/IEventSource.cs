namespace RoadRegistry.Framework
{
    public interface IEventSource 
    {
        void RestoreFromEvents(object[] events);

        object[] TakeEvents();
    }
}