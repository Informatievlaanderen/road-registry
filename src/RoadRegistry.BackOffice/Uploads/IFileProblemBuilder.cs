namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IFileProblemBuilder
{
    IDbaseFileRecordProblemBuilder ThisAtDbaseRecord(RecordNumber number);
    IShapeFileRecordProblemBuilder ThisAtShapeRecord(RecordNumber number);
    IFileErrorBuilder ThisError(string reason);
    IFileWarningBuilder ThisWarning(string reason);
}
