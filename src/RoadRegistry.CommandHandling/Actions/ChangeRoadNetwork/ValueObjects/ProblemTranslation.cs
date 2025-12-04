namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;

public readonly record struct ProblemTranslation(ProblemSeverity Severity, string Code, string Message = "")
{
}
