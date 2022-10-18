namespace RoadRegistry.BackOffice.Uploads;

using Core;

public interface IFileErrorBuilder
{
    FileError Build();
    IFileErrorBuilder WithParameter(ProblemParameter parameter);
    IFileErrorBuilder WithParameters(params ProblemParameter[] parameters);
}