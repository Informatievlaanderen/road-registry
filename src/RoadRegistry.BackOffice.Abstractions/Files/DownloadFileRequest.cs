namespace RoadRegistry.BackOffice.Abstractions.Files
{
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using MediatR;

    public sealed record DownloadFileRequest(string BucketKey, BlobName BlobName, string FileName) : IRequest<DownloadFileResponse>;
}
