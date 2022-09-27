namespace RoadRegistry.BackOffice.Uploads;

using Core;

public interface IFileErrorBuilder
{
    IFileErrorBuilder WithParameter(ProblemParameter parameter);
    IFileErrorBuilder WithParameters(params ProblemParameter[] parameters);
    FileError Build();
}
