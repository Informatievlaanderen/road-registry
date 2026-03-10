namespace RoadRegistry.Infrastructure.DutchTranslations;

using RoadRegistry.Infrastructure.Messages;
using RoadRegistry.ValueObjects.ProblemCodes;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public sealed class ExtractProblemTranslator : ProblemTranslatorBase
{
    public ExtractProblemTranslator()
        : base(new()
        {
            {
                ProblemCode.RoadNode.RoadNodeIsNotAllowed, problem => new(problem.Severity, problem.Reason,
                    $"De wegknoop met {GetRoadNodeIdLabel(problem)} is onterecht.")
            }
        })
    {
    }

    public override ProblemTranslation CreateMissingTranslation(Problem problem)
    {
        return WellKnownProblemTranslators.Default.Translate(problem);
    }

    private static string GetRoadNodeIdLabel(Problem problem, string namePrefix = "Wegknoop")
    {
        return $"WK_OIDN {problem.GetParameterValue($"{namePrefix}Id")}";
    }
}
