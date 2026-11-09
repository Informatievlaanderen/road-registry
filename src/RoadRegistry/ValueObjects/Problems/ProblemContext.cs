namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;

public class ProblemContext
{
    public const string RoadNodeIdParameterName = "WegknoopId";
    public const string OriginalRoadNodeIdParameterName = "WegknoopOorspronkelijkeId";
    public const string RoadSegmentIdParameterName = "WegsegmentId";
    public const string RoadSegmentTempIdsParameterName = "WegsegmentTempIds";
    public const string GradeSeparatedJunctionIdParameterName = "OngelijkGrondseKruisingId";
    public const string GradeJunctionIdParameterName = "GelijkGrondseKruisingId";

    // The names a context parameter can have. Once a problem is flattened into a message, its context parameters are
    // indistinguishable from its other parameters, so these names are how they are recognised again.
    public static readonly IReadOnlyCollection<string> ParameterNames =
    [
        RoadNodeIdParameterName,
        OriginalRoadNodeIdParameterName,
        RoadSegmentIdParameterName,
        RoadSegmentTempIdsParameterName,
        GradeSeparatedJunctionIdParameterName,
        GradeJunctionIdParameterName
    ];

    public required IReadOnlyCollection<ProblemContextParameter> Parameters { get; init; }

    public static ProblemContext For(RoadNodeId roadNodeId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(RoadNodeIdParameterName, roadNodeId.ToInt32())]
        };
    }
    // The identifier the uploader knows a road node by, alongside the one the road network uses. Added to a problem
    // after the fact, because the identifier is only known where the upload was read, not where the problem is raised.
    public static ProblemContext ForOriginalRoadNodeId(RoadNodeId originalRoadNodeId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(OriginalRoadNodeIdParameterName, originalRoadNodeId.ToInt32())]
        };
    }
    public static ProblemContext For(RoadSegmentId roadSegmentId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(RoadSegmentIdParameterName, roadSegmentId.ToInt32())]
        };
    }
    public static ProblemContext For(RoadSegmentIdReference roadSegmentIdReference)
    {
        if (roadSegmentIdReference.TempIds?.Count > 0)
        {
            return new ProblemContext
            {
                Parameters = [
                    new ProblemContextParameter(RoadSegmentIdParameterName, roadSegmentIdReference.RoadSegmentId.ToInt32()),
                    new ProblemContextParameter(RoadSegmentTempIdsParameterName, roadSegmentIdReference.GetTempIdsAsString()),
                ]
            };
        }

        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(RoadSegmentIdParameterName, roadSegmentIdReference.RoadSegmentId.ToInt32())]
        };
    }
    public static ProblemContext For(GradeSeparatedJunctionId gradeSeparatedJunctionId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(GradeSeparatedJunctionIdParameterName, gradeSeparatedJunctionId.ToInt32())]
        };
    }
    public static ProblemContext For(GradeJunctionId gradeJunctionId)
    {
        return new ProblemContext
        {
            Parameters = [new ProblemContextParameter(GradeJunctionIdParameterName, gradeJunctionId.ToInt32())]
        };
    }
}

public sealed record ProblemContextParameter(string Name, object Value);
