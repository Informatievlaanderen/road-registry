namespace RoadRegistry.Infrastructure.Messages;

public readonly record struct ProblemTranslation(ProblemSeverity Severity, string Code, string Message = "")
{
}
