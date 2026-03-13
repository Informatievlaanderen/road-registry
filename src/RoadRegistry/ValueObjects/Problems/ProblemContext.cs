namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;

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
    public static ProblemContext For(RoadSegmentIdReference roadSegmentIdReference)
    {
        if (roadSegmentIdReference.TempIds?.Count > 0)
        {
            return new ProblemContext
            {
                Parameters = [
                    new ProblemContextParameter("WegsegmentId", roadSegmentIdReference.RoadSegmentId.ToInt32()),
                    new ProblemContextParameter("WegsegmentTempIds", roadSegmentIdReference.GetTempIdsAsString()),
                ]
            };
        }

        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("WegsegmentId", roadSegmentIdReference.RoadSegmentId.ToInt32())]
        };
    }
    public static ProblemContext For(GradeSeparatedJunctionId gradeSeparatedJunctionId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("OngelijkGrondseKruisingId", gradeSeparatedJunctionId.ToInt32())]
        };
    }
    public static ProblemContext For(GradeJunctionId gradeJunctionId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter("GelijkGrondseKruisingId", gradeJunctionId.ToInt32())]
        };
    }
}

public sealed record ProblemContextParameter(string Name, object Value);
