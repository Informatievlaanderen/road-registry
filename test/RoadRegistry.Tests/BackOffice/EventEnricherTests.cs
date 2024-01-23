namespace RoadRegistry.Tests.BackOffice;

using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;

public class EnrichEventTests
{
    [Fact]
    public void EachEventIsEnrichedWithTime()
    {
        var instant = NodaConstants.UnixEpoch;
        var expected = InstantPattern.ExtendedIso.Format(instant);
        var clock = new FakeClock(instant);
        var sut = EnrichEvent.WithTime(clock);
        foreach (var eventType in RoadNetworkEvents.All.Union(StreetNameEvents.All))
        {
            var @event = Activator.CreateInstance(eventType);

            sut(@event);

            var whenProp = eventType.GetProperty("When");
            if (whenProp is null)
            {
                Assert.Fail($"Event {eventType.Name} has no 'When' property");
            }

            var value = (string)whenProp.GetValue(@event);
            if (expected != value)
            {
                Assert.Fail($"Handler for event {eventType.Name} is missing");
            }

            Assert.Equal(expected, value);
        }
    }
}
