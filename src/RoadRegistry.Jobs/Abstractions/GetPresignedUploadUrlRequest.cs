namespace RoadRegistry.Jobs.Abstractions
{
    using BackOffice;
    using MediatR;

    public sealed record GetPresignedUploadUrlRequest : IRequest<GetPresignedUploadUrlResponse>
    {
        private GetPresignedUploadUrlRequest()
        {
        }

        public UploadType UploadType { get; init; }
        public DownloadId? DownloadId { get; init; }

        public static GetPresignedUploadUrlRequest ForUploads()
        {
            return new GetPresignedUploadUrlRequest
            {
                UploadType = UploadType.Uploads
            };
        }

        public static GetPresignedUploadUrlRequest ForExtracts(DownloadId downloadId)
        {
            return new GetPresignedUploadUrlRequest
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
