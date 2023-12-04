namespace RoadRegistry.Tests.BackOffice
{
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Framework;

    public class RoadRegistryEventFilterTests
    {
        private enum Event
        {
            _1,
            _2,
            _3
        }

        [Fact]
        public void ExcludeEventsSuccessfully()
        {
            var entity = new DummyEventSourcedEntity(Event._1, Event._2, Event._3);
            var sut = new RoadRegistryEventFilter()
                .Exclude(entity, Event._2);

            Assert.True(sut.IsAllowed(entity, Event._1));
            Assert.False(sut.IsAllowed(entity, Event._2));
            Assert.True(sut.IsAllowed(entity, Event._3));
        }

        private class DummyEventSourcedEntity : IEventSourcedEntity
        {
            private readonly object[] _events;

            public DummyEventSourcedEntity(params object[] events)
            {
                _events = events;
            }

            public void RestoreFromEvents(object[] events)
            {
                throw new NotImplementedException();
            }

            public object[] TakeEvents()
            {
                return _events;
            }
        }
    }
}
