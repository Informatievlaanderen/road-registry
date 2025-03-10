namespace RoadRegistry.BackOffice.Core;

public partial class ImmutableRoadNetworkView
{
    private ImmutableRoadNetworkView With(AddGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(command.Id, new GradeSeparatedJunction(command.Id, command.Type, command.UpperSegmentId, command.LowerSegmentId)),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(ModifyGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.TryReplace(
                command.Id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(command.Type)
                        .WithUpperSegment(command.UpperSegmentId)
                        .WithLowerSegment(command.LowerSegmentId)),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(command.Id),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }
}
