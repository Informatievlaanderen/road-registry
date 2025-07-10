namespace RoadRegistry.BackOffice;

using NodaTime;
using NodaTime.Text;

public static class EnrichEvent
{
    public static EventEnricher WithTime(IClock clock)
    {
        var pattern = InstantPattern.ExtendedIso;

        return @event =>
        {
            switch (@event)
            {
                case IWhen m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
            }
        };
    }
}
