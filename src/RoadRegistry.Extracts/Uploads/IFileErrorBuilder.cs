namespace RoadRegistry.Extracts.Uploads;

using RoadRegistry.ValueObjects.Problems;

public interface IFileErrorBuilder
{
    FileError Build();
    IFileErrorBuilder WithParameter(ProblemParameter parameter);
    IFileErrorBuilder WithParameters(params ProblemParameter[] parameters);
}
