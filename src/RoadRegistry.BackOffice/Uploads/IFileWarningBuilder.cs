namespace RoadRegistry.BackOffice.Uploads;

using Core;
using ValueObjects.Problems;

public interface IFileWarningBuilder
{
    FileWarning Build();
    IFileWarningBuilder WithParameter(ProblemParameter parameter);
    IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);
}
