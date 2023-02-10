namespace RoadRegistry.Hosts.Infrastructure.Modules;

using System;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Configuration;
using Microsoft.Extensions.Configuration;

public class BlobClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var blobOptions = new BlobClientOptions();
            configuration.Bind(blobOptions);
            return blobOptions;
        }).AsSelf().SingleInstance();

        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var s3Options = new S3BlobClientOptions();
            configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);
            return s3Options;
        }).AsSelf().SingleInstance();

        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var fileOptions = new FileBlobClientOptions();
            configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);
            return new FileBlobClient(new DirectoryInfo(fileOptions.Directory));
        }).AsSelf().SingleInstance();

        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var minioServer = configuration.GetValue<string>("MINIO_SERVER");
            if (minioServer != null)
            {
                var accessKey = configuration.GetRequiredValue<string>("MINIO_ACCESS_KEY");
                var secretKey = configuration.GetRequiredValue<string>("MINIO_SECRET_KEY");

                return new AmazonS3Client(
                    new BasicAWSCredentials(accessKey, secretKey),
                    new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                        ServiceURL = minioServer,
                        ForcePathStyle = true
                    }
                );
            }

            return new AmazonS3Client();
        }).AsSelf().SingleInstance();

        IBlobClient CreateBlobClient(IComponentContext c, string bucketKey)
        {
            var blobOptions = c.Resolve<BlobClientOptions>();

            switch (blobOptions.BlobClientType)
            {
                case nameof(S3BlobClient):
                    return new S3BlobClient(c.Resolve<AmazonS3Client>(), c.Resolve<S3BlobClientOptions>().Buckets[bucketKey]);
                case nameof(FileBlobClient):
                    return c.Resolve<FileBlobClient>();
            }

            throw new InvalidOperationException(blobOptions.BlobClientType + " is not a supported blob client type.");
        }

        builder.Register(c => new RoadNetworkUploadsBlobClient(CreateBlobClient(c, WellknownBuckets.UploadsBucket))).AsSelf().SingleInstance();
        builder.Register(c => new RoadNetworkExtractUploadsBlobClient(CreateBlobClient(c, WellknownBuckets.UploadsBucket))).AsSelf().SingleInstance();
        builder.Register(c => new RoadNetworkExtractDownloadsBlobClient(CreateBlobClient(c, WellknownBuckets.ExtractDownloadsBucket))).AsSelf().SingleInstance();
        builder.Register(c => new RoadNetworkFeatureCompareBlobClient(CreateBlobClient(c, WellknownBuckets.FeatureCompareBucket))).AsSelf().SingleInstance();
        builder.Register(c => new RoadNetworkSqsMessagesBlobClient(CreateBlobClient(c, WellknownBuckets.SqsMessagesBucket), c.Resolve<SqsOptions>())).AsSelf().SingleInstance();
    }
}
