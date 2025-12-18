namespace RoadRegistry.Extracts.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IFileProblemBuilder
{
    IDbaseFileRecordProblemBuilder AtDbaseRecord(RecordNumber number);
    IShapeFileRecordProblemBuilder AtShapeRecord(RecordNumber number);
    IFileErrorBuilder Error(string reason);
    IFileWarningBuilder Warning(string reason);
}
