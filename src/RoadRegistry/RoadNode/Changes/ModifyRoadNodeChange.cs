namespace RoadRegistry.RoadNode.Changes;

using ScopedRoadNetwork;

public sealed record ModifyRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }

    // The WK_OIDN the municipality uploaded, when the feature compare matched this road node to a different one in
    // the extract - it matches on geometry, so the two identifiers need not agree. Kept so that a problem about this
    // road node can name the identifier the uploader knows it by.
    public RoadNodeId? OriginalId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public bool? Grensknoop { get; init; }
}
