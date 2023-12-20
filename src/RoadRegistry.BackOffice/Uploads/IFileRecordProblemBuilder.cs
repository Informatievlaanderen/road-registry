namespace RoadRegistry.BackOffice.Uploads;

using Core;

public interface IFileRecordProblemBuilder
{
    IFileErrorBuilder Error(string reason);
    IFileWarningBuilder Warning(string reason);
    IFileRecordProblemBuilder WithParameter(ProblemParameter parameter);
    IFileRecordProblemBuilder WithParameters(params ProblemParameter[] parameters);
}
