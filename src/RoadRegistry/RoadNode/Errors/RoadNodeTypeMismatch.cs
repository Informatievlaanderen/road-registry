namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadNodeTypeMismatch : Error
{
    private RoadNodeTypeMismatch(IReadOnlyCollection<ProblemParameter> parameters)
        : base(ProblemCode.RoadNode.TypeMismatch, parameters)
    {
    }

    public static RoadNodeTypeMismatch New(RoadNodeId node,
        RoadSegmentId[] connectedSegments,
        RoadNodeType actualType,
        RoadNodeType[] expectedTypes)
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
        return new RoadNodeTypeMismatch(parameters);
    }
}
