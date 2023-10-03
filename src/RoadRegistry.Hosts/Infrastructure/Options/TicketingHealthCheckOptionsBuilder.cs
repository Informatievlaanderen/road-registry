namespace RoadRegistry.Hosts.Infrastructure.Options;
using TicketingService.Abstractions;

public class TicketingHealthCheckOptionsBuilder : HealthCheckOptionsBuilder<TicketingHealthCheckOptions>
{
    private ITicketing _ticketingService;

    public override bool IsValid => true;

    public override TicketingHealthCheckOptions Build()
    {
        return new TicketingHealthCheckOptions()
        {
            TicketingService = _ticketingService
        };
    }

    public TicketingHealthCheckOptionsBuilder With(ITicketing ticketingService)
    {
        _ticketingService = ticketingService;
        return this;
    }
}
