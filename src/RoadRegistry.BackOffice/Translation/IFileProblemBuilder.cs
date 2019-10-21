namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IFileProblemBuilder
    {
        IFileErrorBuilder Error(string reason);
        IFileWarningBuilder Warning(string reason);

        IFileDbaseRecordProblemBuilder WithDbaseRecord(RecordNumber number);
        IFileShapeRecordProblemBuilder WithShapeRecord(RecordNumber number);
    }
}