namespace RoadRegistry.Hosts.Infrastructure.Modules;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Microsoft.Extensions.Configuration;
using System.IO;

public class BlobClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterOptions<BlobClientOptions>();
        builder.RegisterOptions<S3BlobClientOptions>();
        builder.RegisterOptions<FileBlobClientOptions>();

        builder.Register(c =>
        {
            var fileOptions = c.Resolve<FileBlobClientOptions>();
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

        builder
            .Register(c => c.Resolve<AmazonS3Client>())
            .As<IAmazonS3>().SingleInstance();

        builder
            .RegisterType<BlobClientFactory>()
            .As<IBlobClientFactory>().SingleInstance();
            
        builder.Register(c => new RoadNetworkUploadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellknownBuckets.UploadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkExtractUploadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellknownBuckets.UploadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkExtractDownloadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellknownBuckets.ExtractDownloadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkFeatureCompareBlobClient(c.Resolve<IBlobClientFactory>().Create(WellknownBuckets.FeatureCompareBucket))).SingleInstance();
    }
}
