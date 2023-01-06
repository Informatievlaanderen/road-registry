namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Options;

public sealed class TicketingOptions
{
    public const string ConfigurationKey = "TicketingService";

    public string InternalBaseUrl { get; set; }
}
