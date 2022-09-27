namespace RoadRegistry.BackOffice.Framework;

public interface IEventSourcedEntity
{
    void RestoreFromEvents(object[] events);

    object[] TakeEvents();
}
