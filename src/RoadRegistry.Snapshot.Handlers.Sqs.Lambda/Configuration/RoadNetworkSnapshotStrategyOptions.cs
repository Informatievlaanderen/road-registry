namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Configuration;

using BackOffice;

public class RoadNetworkSnapshotStrategyOptions: IHasConfigurationKey
{
    public int EventCount { get; set; } = 50;

    public int GetLastAllowedStreamVersionToTakeSnapshot(int streamVersion /* Eg. 18499 */)
    {
        var streamDifference = streamVersion % EventCount; // Eg. 49
        var streamMaxVersion = streamVersion - streamDifference; // Eg. 18450

        return streamMaxVersion;
    }

    public string GetConfigurationKey()
    {
        return "SnapshotOptions";
    }
}
