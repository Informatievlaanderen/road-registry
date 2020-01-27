namespace RoadRegistry.BackOffice
{
    using System;
    using Messages;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Text;
    using Xunit;

    public class EnrichEventTests
    {
        [Fact]
        public void EachEventIsEnrichedWithTime()
        {
            var instant = NodaConstants.UnixEpoch;
            var expected = InstantPattern.ExtendedIso.Format(instant);
            var clock = new FakeClock(instant);
            var sut = EnrichEvent.WithTime(clock);
            foreach (var eventType in RoadNetworkEvents.All)
            {
                var @event = Activator.CreateInstance(eventType);

                sut(@event);

                var value = (string)eventType.GetProperty("When").GetValue(@event);
                Assert.Equal(expected, value);
            }

        }
    }
}
