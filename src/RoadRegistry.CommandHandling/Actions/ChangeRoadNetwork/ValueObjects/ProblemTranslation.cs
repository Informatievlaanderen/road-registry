namespace RoadRegistry.BackOffice.Core;

public readonly record struct ProblemTranslation(Messages.ProblemSeverity Severity, string Code, string Message = "")
{
}
