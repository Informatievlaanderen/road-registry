namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Configuration;

public class RoadNetworkSnapshotStrategyOptions
{
    public const string ConfigurationSection = "SnapshotOptions";
    public int EventCount { get; set; } = 50;
}
