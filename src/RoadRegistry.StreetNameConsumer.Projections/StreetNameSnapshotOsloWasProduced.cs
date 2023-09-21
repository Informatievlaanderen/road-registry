namespace RoadRegistry.StreetNameConsumer.Projections;

using StreetName;

public class StreetNameSnapshotOsloWasProduced
{
    public string StreetNameId { get; init; }
    public long Offset { get; init; }
    public StreetNameSnapshotOsloRecord Record { get; init; }
}
