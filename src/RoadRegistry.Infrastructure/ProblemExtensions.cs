namespace RoadRegistry.Infrastructure;

using RoadRegistry.ValueObjects.Problems;
using Problem = Messages.Problem;
using ProblemParameter = Messages.ProblemParameter;
using ProblemSeverity = Messages.ProblemSeverity;

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

    public static ValueObjects.Problems.ProblemSeverity Translate(this ProblemSeverity value) => value switch
    {
        ProblemSeverity.Error => ValueObjects.Problems.ProblemSeverity.Error,
        ProblemSeverity.Warning => ValueObjects.Problems.ProblemSeverity.Warning,
        _ => throw new NotSupportedException()
    };
}
