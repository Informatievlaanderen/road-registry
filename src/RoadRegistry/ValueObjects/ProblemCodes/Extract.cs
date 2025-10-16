namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Extract
    {
        public static readonly ProblemCode NotFound = new("ExtractNotFound");
        public static readonly ProblemCode ContourIsRequired = new("ExtractContourIsRequired");
        public static readonly ProblemCode ContourInvalid = new("ExtractContourInvalid");
        public static readonly ProblemCode BeschrijvingIsRequired = new("ExtractBeschrijvingIsRequired");
        public static readonly ProblemCode BeschrijvingTooLong = new("ExtractBeschrijvingTooLong");
        public static readonly ProblemCode ExterneIdInvalid = new("ExtractExterneIdInvalid");
        public static readonly ProblemCode ProjectionInvalid  = new("ExtractProjectionInvalid");
        public static readonly ProblemCode NisCodeIsRequired = new("ExtractNisCodeIsRequired");
        public static readonly ProblemCode NisCodeInvalid = new("ExtractNisCodeInvalid");
        public static readonly ProblemCode CanNotUploadForSupersededDownload = new("ExtractCanNotUploadForSupersededDownload");
        public static readonly ProblemCode ExtractHasNotBeenDownloaded = new("ExtractExtractHasNotBeenDownloaded");
        public static readonly ProblemCode CanNotUploadForInformativeExtract = new("ExtractCanNotUploadForInformativeExtract");
        public static readonly ProblemCode CanNotUploadForClosedExtract = new("ExtractCanNotUploadForClosedExtract");
        public static readonly ProblemCode DownloadIdIsRequired = new("ExtractDownloadIdIsRequired");
        public static readonly ProblemCode CorruptArchive = new("ExtractCorruptArchive");
    }
}
