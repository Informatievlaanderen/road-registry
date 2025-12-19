namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;
using System.Linq;

public class ProblemContext
{
    public required IReadOnlyCollection<ProblemContextParameter> Parameters { get; init; }

    public static ProblemContext For(RoadNodeId roadNodeId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("WegknoopId", roadNodeId.ToInt32())]
        };
    }
    public static ProblemContext For(RoadSegmentId roadSegmentId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("WegsegmentId", roadSegmentId.ToInt32())]
        };
    }
    public static ProblemContext For(GradeSeparatedJunctionId gradeSeparatedJunctionId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("OngelijkGrondseKruisingId", gradeSeparatedJunctionId.ToInt32())]
        };
    }
}

public sealed record ProblemContextParameter(string Name, object Value);
