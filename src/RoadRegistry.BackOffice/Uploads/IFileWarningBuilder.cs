namespace RoadRegistry.BackOffice.Uploads;

using Core;

public interface IFileWarningBuilder
{
    FileWarning Build();
    IFileWarningBuilder WithParameter(ProblemParameter parameter);
    IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);
}
