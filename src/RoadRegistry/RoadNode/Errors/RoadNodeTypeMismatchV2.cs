namespace RoadRegistry.RoadNode.Errors;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public class RoadNodeTypeV2Mismatch : Error
{
    private RoadNodeTypeV2Mismatch(IReadOnlyCollection<ProblemParameter> parameters)
        : base(ProblemCode.RoadNode.TypeV2Mismatch, parameters)
    {
    }

    public static RoadNodeTypeV2Mismatch New(RoadNodeId node,
        RoadSegmentId[] connectedSegments,
        RoadNodeTypeV2 actualType,
        RoadNodeTypeV2[] expectedTypes)
    {
        if (connectedSegments == null)
            throw new ArgumentNullException(nameof(connectedSegments));
        if (expectedTypes == null)
            throw new ArgumentNullException(nameof(expectedTypes));
        if (expectedTypes.Length == 0)
            throw new ArgumentException("The expected road node types must contain at least one.", nameof(expectedTypes));

        var parameters = new List<ProblemParameter>
        {
            new("RoadNodeId",
                node.ToInt32().ToString()),
            new("ConnectedSegmentCount",
                connectedSegments.Length.ToString(CultureInfo.InvariantCulture))
        };
        parameters.AddRange(connectedSegments.Select(segment => new ProblemParameter("ConnectedSegmentId", segment.ToInt32().ToString())));
        parameters.Add(new ProblemParameter("Actual", actualType.ToString()));
        parameters.AddRange(expectedTypes.Select(type => new ProblemParameter("Expected", type.ToString())));
        return new RoadNodeTypeV2Mismatch(parameters);
    }
}
