namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Upload
    {
        public static readonly ProblemCode CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload = new("CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload");
        public static readonly ProblemCode CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce = new("CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce");
        public static readonly ProblemCode UploadNotAllowedForInformativeExtract = new("UploadNotAllowedForInformativeExtract");
        public static readonly ProblemCode UnexpectedError = new("UploadUnexpectedError");
        public static readonly ProblemCode UnsupportedMediaType = new("UploadUnsupportedMediaType");
        public static readonly ProblemCode DownloadIdIsRequired = new("UploadDownloadIdIsRequired");
    }
}
