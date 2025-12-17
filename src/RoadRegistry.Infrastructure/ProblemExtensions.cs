namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using RoadRegistry.ValueObjects.Problems;
using Problem = ValueObjects.Problem;
using ProblemParameter = ValueObjects.ProblemParameter;
using ProblemSeverity = ValueObjects.ProblemSeverity;

public static class ProblemExtensions
{
    public static Problem Translate(this RoadRegistry.ValueObjects.Problems.Problem problem)
    {
        return new Problem
        {
            Severity = problem is Error
                ? ProblemSeverity.Error
                : ProblemSeverity.Warning,
            Reason = problem.Reason,
            Parameters = problem.Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }

    public static ProblemParameter Translate(this RoadRegistry.ValueObjects.Problems.ProblemParameter problemParameter)
    {
        return new ProblemParameter
        {
            Name = problemParameter.Name,
            Value = problemParameter.Value
        };
    }

    public static RoadRegistry.ValueObjects.ProblemSeverity Translate(this ProblemSeverity value) => value switch
    {
        ProblemSeverity.Error => RoadRegistry.ValueObjects.ProblemSeverity.Error,
        ProblemSeverity.Warning => RoadRegistry.ValueObjects.ProblemSeverity.Warning,
        _ => throw new NotSupportedException()
    };
}
