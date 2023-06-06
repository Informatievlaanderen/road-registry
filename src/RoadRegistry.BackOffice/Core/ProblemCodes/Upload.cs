namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Upload
    {
        public static readonly ProblemCode CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload = new("CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload");
        public static readonly ProblemCode CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce = new("CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce");
        public static readonly ProblemCode UploadNotAllowedForInformativeExtract = new("UploadNotAllowedForInformativeExtract");
    }
}
