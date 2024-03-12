namespace RoadRegistry.Jobs.Abstractions
{
    using BackOffice;
    using MediatR;

    public sealed record UploadPreSignedUrlRequest : IRequest<UploadPreSignedUrlResponse>
    {
        private UploadPreSignedUrlRequest()
        {
        }

        public UploadType UploadType { get; init; }
        public DownloadId? DownloadId { get; init; }

        public static UploadPreSignedUrlRequest ForUploads()
        {
            return new UploadPreSignedUrlRequest
            {
                UploadType = UploadType.Uploads
            };
        }

        public static UploadPreSignedUrlRequest ForExtracts(DownloadId downloadId)
        {
            return new UploadPreSignedUrlRequest
            {
                UploadType = UploadType.Extracts,
                DownloadId = downloadId
            };
        }
    }

    public enum UploadType
    {
        Uploads,
        Extracts
    }
}
