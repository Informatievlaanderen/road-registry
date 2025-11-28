namespace RoadRegistry.BackOffice.Uploads;

using Core;
using ValueObjects.Problems;

public interface IFileRecordProblemBuilder
{
    IFileErrorBuilder Error(string reason);
    IFileWarningBuilder Warning(string reason);
    IFileRecordProblemBuilder WithParameter(ProblemParameter parameter);
    IFileRecordProblemBuilder WithParameters(params ProblemParameter[] parameters);
}
