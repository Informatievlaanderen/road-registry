namespace RoadRegistry.Projector.Projections;

public class ProjectionStatus
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required long CurrentPosition { get; set; }
    public required long StorePosition { get; set; }
    // What the projection is meant to be doing ("subscribed" / "stopped"). The status page renders its start/stop
    // control from this, so it is intent, not observation.
    public required string State { get; set; }

    // What the shard is actually doing right now. Only the Marten projections can report this; for the stream-store
    // projections it stays empty, as it always did.
    public string ActualState { get; set; } = string.Empty;

    public required string ErrorMessage { get; set; }
}
