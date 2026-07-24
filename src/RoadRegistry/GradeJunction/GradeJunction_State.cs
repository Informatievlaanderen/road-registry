namespace RoadRegistry.GradeJunction;

using Events.V2;
using Newtonsoft.Json;
using RoadRegistry.Extensions;

public partial class GradeJunction : MartenAggregateRootEntity<GradeJunctionId>
{
    public GradeJunctionId GradeJunctionId { get; }
    public RoadSegmentId RoadSegmentId1 { get; private set; }
    public RoadSegmentId RoadSegmentId2 { get; private set; }
    public JunctionGeometry? Geometry { get; private set; }

    public bool IsRemoved { get; private set; }

    private readonly string? _lastSnapshotEventHash;
    public string LastEventHash => UncommittedEvents.Count > 0 ? UncommittedEvents[^1].GetHash() : _lastSnapshotEventHash ?? string.Empty;

    public GradeJunction(GradeJunctionId id)
        : base(id)
    {
        GradeJunctionId = id;
    }

    [JsonConstructor]
    protected GradeJunction(
        int gradeJunctionId,
        int roadSegmentId1,
        int roadSegmentId2,
        JunctionGeometry? geometry,
        bool isRemoved,
        string? lastEventHash
    )
        : this(new GradeJunctionId(gradeJunctionId))
    {
        RoadSegmentId1 = new RoadSegmentId(roadSegmentId1);
        RoadSegmentId2 = new RoadSegmentId(roadSegmentId2);
        Geometry = geometry;
        IsRemoved = isRemoved;
        _lastSnapshotEventHash = lastEventHash;
    }

    public static GradeJunction Create(GradeJunctionWasAdded @event)
    {
        var junction = new GradeJunction(@event.GradeJunctionId)
        {
            RoadSegmentId1 = @event.RoadSegmentId1,
            RoadSegmentId2 = @event.RoadSegmentId2,
            Geometry = @event.Geometry
        };
        junction.UncommittedEvents.Add(@event);
        return junction;
    }

    public void Apply(GradeJunctionWasModified @event)
    {
        UncommittedEvents.Add(@event);

        RoadSegmentId1 = @event.RoadSegmentId1 ?? RoadSegmentId1;
        RoadSegmentId2 = @event.RoadSegmentId2 ?? RoadSegmentId2;
    }

    public void Apply(GradeJunctionGeometryWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry;
    }

    public void Apply(GradeJunctionWasRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }
}
