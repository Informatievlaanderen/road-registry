namespace RoadRegistry.Hosts.Infrastructure.Options;

public sealed class TicketingOptions
{
    public const string ConfigurationKey = "TicketingService";

    public string InternalBaseUrl { get; set; }
}
