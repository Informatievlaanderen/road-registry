namespace RoadRegistry.Hosts.Infrastructure.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.Runtime.Internal.Auth;
    using Amazon.S3;
    using Microsoft.Extensions.Logging;

    public interface IAmazonS3Extended : IAmazonS3
    {
        public CreatePresignedPostResponse CreatePresignedPost(CreatePresignedPostRequest request);
    }

    /// <summary>
    /// An extended Amazon S3 client, with added support for creating presigned posts.
    /// </summary>
    /// <seealso href="https://github.com/aws/aws-sdk-net/issues/1901">
    /// AmazonS3Client is missing Create Presigned Post
    /// </seealso>
    public sealed class AmazonS3ExtendedClient : AmazonS3Client, IAmazonS3Extended
    {
        private readonly ILogger<AmazonS3ExtendedClient> _logger;

        public AmazonS3ExtendedClient(
            ILoggerFactory loggerFactory,
            AmazonS3Config config)
            : base(config)
        {
            _logger = loggerFactory.CreateLogger<AmazonS3ExtendedClient>();
        }

        public AmazonS3ExtendedClient(
            ILoggerFactory loggerFactory,
            AWSCredentials credentials,
            AmazonS3Config config
        ) : base(credentials, config)
        {
            _logger = loggerFactory.CreateLogger<AmazonS3ExtendedClient>();
        }

        public CreatePresignedPostResponse CreatePresignedPost(CreatePresignedPostRequest request)
        {
            var regionName = GetRegionName();
            _logger.LogInformation($"Region: {regionName}");

            var url = new Uri($"{GetS3BaseUrl()}/{request.BucketName}");

            _logger.LogInformation($"BucketUrl: {url}");

            var signingDate = Config.CorrectedUtcNow
                .ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);

            var shortDate = signingDate[..8];

            var credentials = Credentials.GetCredentials()
                ?? throw new ConfigurationErrorsException("No credentials found");

            _logger.LogInformation($"credentials: {credentials.AccessKey}");
            _logger.LogInformation($"usetoken: {credentials.UseToken}");

            var credentialScope = $"{shortDate}/{regionName}/s3/aws4_request";

            var fields = new Dictionary<string, string>
            {
                { "key", request.Key },
                { "bucket", request.BucketName },
                { "X-Amz-Algorithm", "AWS4-HMAC-SHA256" },
                { "X-Amz-Credential", $"{credentials.AccessKey}/{credentialScope}" },
                { "X-Amz-Date", signingDate },
                { "X-Amz-Security-Token", credentials.Token }
            };

            foreach (var (key, value) in fields)
            {
                request.Conditions.Add(new ExactMatchCondition(key, value));
            }

            var postPolicy = new PostPolicy(
                Config.CorrectedUtcNow.Add(request.Expires ?? TimeSpan.FromSeconds(3600)),
                request.Conditions);

            var postPolicyJson = JsonSerializer.Serialize(
                postPolicy,
                AmazonS3SerializerContext.Default.PostPolicy);

            var postPolicyEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(postPolicyJson));

            fields.Add("Policy", postPolicyEncoded);

            var signingKey = AWS4Signer.ComposeSigningKey(
                credentials.SecretKey,
                regionName,
                shortDate,
                "s3");

            var signature =
                AWS4Signer.ComputeKeyedHash(SigningAlgorithm.HmacSHA256, signingKey, postPolicyEncoded);

            fields.Add("X-Amz-Signature", Convert.ToHexString(signature).ToLowerInvariant());

            return new CreatePresignedPostResponse(url, fields);
        }

        private string GetS3BaseUrl()
        {
            if (Config.ServiceURL is not null)
            {
                return $"{Config.ServiceURL.TrimEnd('/')}/api/v1/buckets";
            }

            var regionName = GetRegionName();
            return $"https://s3.{regionName}.amazonaws.com";
        }

        private string GetRegionName()
        {
            return Config.RegionEndpoint?.SystemName ?? RegionEndpoint.EUWest1.SystemName;
        }
    }

    public sealed record CreatePresignedPostRequest(
        string BucketName,
        string Key,
        IList<ExactMatchCondition> Conditions,
        TimeSpan? Expires);

    public sealed record CreatePresignedPostResponse(Uri Url, Dictionary<string, string> Fields);

    public sealed record PostPolicy(DateTime Expiration, IList<ExactMatchCondition> Conditions);

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(PostPolicy))]
    public sealed partial class AmazonS3SerializerContext : JsonSerializerContext
    {
    }

    [JsonConverter(typeof(ExactMatchConditionConverter))]
    public sealed record ExactMatchCondition(string Key, string Value); //: Condition;

    public sealed class ExactMatchConditionConverter : JsonConverter<ExactMatchCondition>
    {
        public override ExactMatchCondition? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            ExactMatchCondition value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(value.Key, value.Value);
            writer.WriteEndObject();
        }
    }
}
