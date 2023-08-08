using RoadRegistry.Hosts.Infrastructure.Options;

namespace RoadRegistry.BackOffice.Api.Tests
{
    public class FakeTicketingOptions: TicketingOptions
    {
        public FakeTicketingOptions()
        {
            InternalBaseUrl = "http://internal/tickets";
            PublicBaseUrl = "http://public/tickets";
        }
    }
}
