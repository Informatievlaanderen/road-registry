namespace RoadRegistry.BackOffice.Core;

public static class ValueObjectExtensions
{
    public static RoadRegistry.BackOffice.Messages.Problem Translate(this Problem problem)
    {
        return new RoadRegistry.BackOffice.Messages.Problem
        {
            Severity = problem is Error
                ? BackOffice.Messages.ProblemSeverity.Error
                : BackOffice.Messages.ProblemSeverity.Warning,
            Reason = problem.Reason,
            Parameters = problem.Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }

    public static RoadRegistry.BackOffice.Messages.ProblemParameter Translate(this ProblemParameter problemParameter)
    {
        return new RoadRegistry.BackOffice.Messages.ProblemParameter
        {
            Name = problemParameter.Name,
            Value = problemParameter.Value
        };
    }

    public static Core.ProblemSeverity Translate(this Messages.ProblemSeverity value) => value switch
    {
        Messages.ProblemSeverity.Error => ProblemSeverity.Error,
        Messages.ProblemSeverity.Warning => ProblemSeverity.Warning,
        _ => throw new NotSupportedException()
    };
}
