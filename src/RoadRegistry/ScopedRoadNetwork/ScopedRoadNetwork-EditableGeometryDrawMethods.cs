namespace RoadRegistry.ScopedRoadNetwork;

using RoadRegistry.RoadSegment;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // Whether the caller holds the scope that goes with this road segment's geometry draw method. The problem names
    // the road segment itself, because the one that is out of reach is not always the one the request names.
    private static Problems ValidateGeometryDrawMethodIsEditable(
        RoadSegment roadSegment,
        EditableGeometryDrawMethods editableGeometryDrawMethods)
    {
        var geometryDrawMethod = roadSegment.Attributes?.GeometryDrawMethod;
        if (editableGeometryDrawMethods.Allows(geometryDrawMethod))
        {
            return Problems.None;
        }

        return Problems.Single(geometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
            ? (Error)new RoadSegmentMeasuredNotAllowed(roadSegment.RoadSegmentId)
            : new RoadSegmentOutlinedNotAllowed(roadSegment.RoadSegmentId));
    }
}
