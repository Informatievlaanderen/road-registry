namespace RoadRegistry.Road
{
    using Events;
    using ValueObjects;

    public partial class Road
    {
        private RoadId _roadId;

        private Road()
        {
            Register<RoadWasRegistered>(When);
        }

        private void When(RoadWasRegistered @event)
        {
            _roadId = new RoadId(@event.RoadId);
        }
    }
}
