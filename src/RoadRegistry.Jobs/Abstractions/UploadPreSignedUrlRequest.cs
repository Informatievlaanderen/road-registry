namespace RoadRegistry.Jobs.Abstractions
{
    using MediatR;

    public sealed record UploadPreSignedUrlRequest(UploadType Type, string? DownloadId = null) : IRequest<UploadPreSignedUrlResponse>;

    public enum UploadType
    {
        Uploads,
        Extracts
    }
}
