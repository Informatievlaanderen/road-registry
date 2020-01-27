namespace RoadRegistry.BackOffice.Uploads
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IFileProblemBuilder
    {
        IFileErrorBuilder Error(string reason);
        IFileWarningBuilder Warning(string reason);

        IDbaseFileRecordProblemBuilder AtDbaseRecord(RecordNumber number);
        IShapeFileRecordProblemBuilder AtShapeRecord(RecordNumber number);
    }
}
