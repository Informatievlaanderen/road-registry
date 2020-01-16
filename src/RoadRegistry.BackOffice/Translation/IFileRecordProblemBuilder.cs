namespace RoadRegistry.BackOffice.Translation
{
    public interface IFileRecordProblemBuilder
    {
        IFileErrorBuilder Error(string reason);
        IFileWarningBuilder Warning(string reason);
    }
}