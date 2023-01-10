namespace RoadRegistry.BackOffice.Api.Infrastructure.Options;

public sealed class TicketingOptions
{
    public const string ConfigurationKey = "TicketingService";

    public string InternalBaseUrl { get; set; }
    public string PublicBaseUrl { get; set; }
}
