namespace RoadRegistry.BackOffice.Core;

public readonly record struct ProblemTranslation(ProblemSeverity Severity, string Code, string Message = "")
{
}
