namespace RoadRegistry.BackOffice.Abstractions.Uploads
{
    using MediatR;

    public sealed record UploadPreSignedUrlRequest : IRequest<UploadPreSignedUrlResponse>;
}
