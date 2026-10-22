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
        var label = $"WK_OIDN {problem.GetParameterValue($"{namePrefix}Id")}";

        // The feature compare matches road nodes on geometry, so the one the register uses need not be the one the
        // upload carried. Both are named, or the uploader has nothing to look the road node up by.
        var originalId = problem.GetOptionalParameterValue($"{namePrefix}OorspronkelijkeId");

        return originalId is not null ? $"{label} (origineel {originalId})" : label;
    }

    protected override string GetGradeSeparatedJunctionIdLabel(Problem problem, string namePrefix = "OngelijkGrondseKruising")
    {
        return $"OK_OIDN {problem.GetParameterValue($"{namePrefix}Id")}";
    }
}
