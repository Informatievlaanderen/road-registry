namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using MediatR;
    using RoadRegistry.BackOffice;

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

        public static GetPresignedUploadUrlRequest ForExtractsV2(DownloadId downloadId)
        {
            return new GetPresignedUploadUrlRequest
            {
                UploadType = UploadType.ExtractsV2,
                DownloadId = downloadId
            };
        }
    }

    public enum UploadType
    {
        Uploads,
        Extracts,
        ExtractsV2
    }
}
