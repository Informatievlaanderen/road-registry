namespace RoadRegistry.BackOffice.Uploads;

public interface IFileRecordProblemBuilder
{
    IFileErrorBuilder ThisError(string reason);
    IFileWarningBuilder ThisWarning(string reason);
}
