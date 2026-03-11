namespace RoadRegistry.Infrastructure.DutchTranslations;

using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public sealed class ExtractProblemTranslator : DefaultProblemTranslator
{
    protected override string GetRoadSegmentIdLabel(Problem problem, string namePrefix = "Wegsegment")
    {
        if (problem.HasParameter($"{namePrefix}TempIds"))
        {
            return $"WS_TEMPID ({problem.GetParameterValue($"{namePrefix}TempIds")})";
        }

        return $"WS_OIDN {problem.GetParameterValue($"{namePrefix}Id")}";
    }

    protected override string GetRoadNodeIdLabel(Problem problem, string namePrefix = "Wegknoop")
    {
        return $"WK_OIDN {problem.GetParameterValue($"{namePrefix}Id")}";
    }
}
