namespace RoadRegistry.Extracts.Uploads;

using RoadRegistry.ValueObjects.Problems;

public interface IFileWarningBuilder
{
    FileWarning Build();
    IFileWarningBuilder WithParameter(ProblemParameter parameter);
    IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);
}
