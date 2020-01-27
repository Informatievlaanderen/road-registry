namespace RoadRegistry.BackOffice.Uploads
{
    public interface IFileRecordProblemBuilder
    {
        IFileErrorBuilder Error(string reason);
        IFileWarningBuilder Warning(string reason);
    }
}
