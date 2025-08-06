namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using Hosts.Infrastructure.Modules;
    using Jobs;
    using Microsoft.Extensions.Options;
    using Options;
    using System;
    using System.Collections.Generic;

    public interface IJobUploadUrlPresigner
    {
        CreatePresignedPostResponse CreatePresignedUploadUrl(Job job);
    }

    public class AmazonS3JobUploadUrlPresigner : IJobUploadUrlPresigner
    {
        private readonly IAmazonS3Extended _s3Extended;
        private readonly JobsBucketOptions _bucketOptions;

        public AmazonS3JobUploadUrlPresigner(
            IAmazonS3Extended s3Extended,
            IOptions<JobsBucketOptions> bucketOptions)
        {
            _s3Extended = s3Extended;
            _bucketOptions = bucketOptions.Value;
        }

        public CreatePresignedPostResponse CreatePresignedUploadUrl(Job job)
        {
            var response = _s3Extended.CreatePresignedPost(
                new CreatePresignedPostRequest(
                    _bucketOptions.BucketName,
                job.UploadBlobName,
                    new List<ExactMatchCondition>(),
                    TimeSpan.FromMinutes(_bucketOptions.UrlExpirationInMinutes)));
            return response;
        }
    }

    public class AnonymousBackOfficeApiJobUploadUrlPresigner : IJobUploadUrlPresigner
    {
        private readonly ApiOptions _apiOptions;

        public AnonymousBackOfficeApiJobUploadUrlPresigner(ApiOptions apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public CreatePresignedPostResponse CreatePresignedUploadUrl(Job job)
        {
            var uri = new Uri($"{_apiOptions.BaseUrl}/v1/files/upload/{job.Id}");
            return new CreatePresignedPostResponse(uri, new Dictionary<string, string>());
        }
    }
}
