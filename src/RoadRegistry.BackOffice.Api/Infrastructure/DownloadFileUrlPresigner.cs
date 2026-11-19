namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;
    using BackOffice.Configuration;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using NodaTime;
    using Options;

    public interface IDownloadFileUrlPresigner
    {
        Task<CreatePresignedGetResponse> CreatePresignedDownloadUrl(string bucketKey, BlobName blobName, string fileName);
    }

    public sealed record CreatePresignedGetResponse(Uri Url, string FileName);

    public class AmazonS3DownloadFileUrlPresigner : IDownloadFileUrlPresigner
    {
        private readonly IClock _clock;
        private readonly IAmazonS3 _amazonS3;
        private readonly S3Options _s3Options;
        private readonly S3BlobClientOptions _s3BlobClientOptions;

        public AmazonS3DownloadFileUrlPresigner(
            IClock clock,
            IAmazonS3 amazonS3,
            S3Options s3Options,
            S3BlobClientOptions s3BlobClientOptions)
        {
            _clock = clock;
            _amazonS3 = amazonS3;
            _s3Options = s3Options;
            _s3BlobClientOptions = s3BlobClientOptions;
        }

        public async Task<CreatePresignedGetResponse> CreatePresignedDownloadUrl(string bucketKey, BlobName blobName, string fileName)
        {
            var bucketName = _s3BlobClientOptions.GetBucketName(bucketKey);

            var preSignedUrl = await _amazonS3.GetPreSignedURLAsync(
                new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = blobName,
                    Expires = _clock
                        .GetCurrentInstant()
                        .Plus(Duration.FromSeconds(_s3Options.ExpiresInSeconds))
                        .ToDateTimeUtc(),
                    // The url points to another origin than the portal, so a browser ignores the download attribute
                    // the portal puts on the link and names the file after the last segment of the url - the blob
                    // name, which for an upload is its id without an extension. Only a Content-Disposition header on
                    // the response itself makes the browser use this name, and S3 sends it when the url asks for it.
                    ResponseHeaderOverrides = new ResponseHeaderOverrides
                    {
                        ContentDisposition = $"attachment; filename=\"{fileName}\""
                    }
                });
            return new CreatePresignedGetResponse(new Uri(preSignedUrl), fileName);
        }
    }

    public class AnonymousBackOfficeApiDownloadFileUrlPresigner : IDownloadFileUrlPresigner
    {
        private readonly ApiOptions _apiOptions;

        public AnonymousBackOfficeApiDownloadFileUrlPresigner(ApiOptions apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public Task<CreatePresignedGetResponse> CreatePresignedDownloadUrl(string bucketKey, BlobName blobName, string fileName)
        {
            var uri = new Uri($"{_apiOptions.BaseUrl}/v1/files/download?bucket={bucketKey}&blob={blobName}&fileName={Uri.EscapeDataString(fileName)}");
            return Task.FromResult(new CreatePresignedGetResponse(uri, fileName));
        }
    }
}
