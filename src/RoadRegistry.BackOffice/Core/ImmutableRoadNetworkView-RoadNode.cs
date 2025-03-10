namespace RoadRegistry.BackOffice.Core;

public partial class ImmutableRoadNetworkView
{
    private ImmutableRoadNetworkView With(AddRoadNode command)
    {
        var version = RoadNodeVersion.Initial;
        var node = new RoadNode(command.Id, version, command.Type, command.Geometry);
        return new ImmutableRoadNetworkView(
            _nodes.Add(command.Id, node),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(ModifyRoadNode command)
    {
        return new ImmutableRoadNetworkView(
            _nodes.TryReplace(command.Id, node => node.WithGeometry(command.Geometry).WithType(command.Type)),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadNode command)
    {
        return new ImmutableRoadNetworkView(
            _nodes.Remove(command.Id),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }
}
