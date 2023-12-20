namespace RoadRegistry.Hosts.Infrastructure.Options;

using TicketingService.Abstractions;

public class TicketingHealthCheckOptions
{
    public ITicketing TicketingService { get; init; }
}
