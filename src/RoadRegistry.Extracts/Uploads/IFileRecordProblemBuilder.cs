namespace RoadRegistry.Extracts.Uploads;

using RoadRegistry.ValueObjects.Problems;

public interface IFileRecordProblemBuilder
{
    IFileErrorBuilder Error(string reason);
    IFileWarningBuilder Warning(string reason);
    IFileRecordProblemBuilder WithParameter(ProblemParameter parameter);
    IFileRecordProblemBuilder WithParameters(params ProblemParameter[] parameters);
}
