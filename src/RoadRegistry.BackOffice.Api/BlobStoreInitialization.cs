namespace RoadRegistry.Api
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Threading.Tasks;
    using Amazon.S3;
    using AspNetCore.AsyncInitialization;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class BlobStoreInitialization : IAsyncInitializer
    {
        private readonly IBlobClient _client;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public BlobStoreInitialization(IBlobClient client, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InitializeAsync()
        {
            switch (_client)
            {
                case S3BlobClient _:
                    if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
                    {
                        var s3Client = _serviceProvider.GetService<AmazonS3Client>();
                        var s3Options = new S3BlobClientOptions();
                        _configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                        var buckets = await s3Client.ListBucketsAsync();
                        if (!buckets.Buckets.Exists(bucket => bucket.BucketName == s3Options.BucketPrefix + WellknownBuckets.UploadsBucket))
                        {
                            await s3Client.PutBucketAsync(s3Options.BucketPrefix + WellknownBuckets.UploadsBucket);
                        }
                    }
                    break;
                case FileBlobClient _:
                    var fileOptions = new FileBlobClientOptions();
                    _configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                    if (!Directory.Exists(fileOptions.Directory))
                    {
                        Directory.CreateDirectory(fileOptions.Directory);
                    }
                    break;
            }
        }
    }
}
