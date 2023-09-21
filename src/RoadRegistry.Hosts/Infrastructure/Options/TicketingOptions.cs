using RoadRegistry.BackOffice;

namespace RoadRegistry.Hosts.Infrastructure.Options;

public class TicketingOptions: IHasConfigurationKey
{
    public string InternalBaseUrl { get; set; }
    public string PublicBaseUrl { get; set; }

    public string GetConfigurationKey()
    {
        return "TicketingService";
    }
}
