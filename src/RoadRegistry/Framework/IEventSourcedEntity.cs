namespace RoadRegistry.Framework
{
    public interface IEventSourcedEntity
    {
        void RestoreFromEvents(object[] events);

        object[] TakeEvents();
    }
}
