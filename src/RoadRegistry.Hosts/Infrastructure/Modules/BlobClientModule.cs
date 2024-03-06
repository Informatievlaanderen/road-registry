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
using Be.Vlaanderen.Basisregisters.EventHandling;

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

        builder
            .Register(c =>
            {
                var configuration = c.Resolve<IConfiguration>();

                var s3Configuration = configuration.GetOptions<S3Options>();
                var s3OptionsJsonSerializer = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

                if (s3Configuration?.ServiceUrl is not null)
                {
                    var developments3Configuration = configuration.GetOptions<DevelopmentS3Options>();
                    return new DevelopmentS3Options(s3OptionsJsonSerializer, developments3Configuration);
                }

                return new S3Options(s3OptionsJsonSerializer);
            })
            .As<S3Options>()
            .SingleInstance();

        builder.Register(c => c.Resolve<S3Options>().CreateS3Client()).AsSelf().SingleInstance();

        builder
            .Register(c => c.Resolve<AmazonS3ExtendedClient>())
            .As<IAmazonS3Extended>().SingleInstance();
        builder
            .Register(c => c.Resolve<IAmazonS3Extended>())
            .As<IAmazonS3>().SingleInstance();

        builder
            .RegisterType<BlobClientFactory>()
            .As<IBlobClientFactory>().SingleInstance();
            
        builder.Register(c => new RoadNetworkUploadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellKnownBuckets.UploadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkExtractUploadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellKnownBuckets.UploadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkExtractDownloadsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellKnownBuckets.ExtractDownloadsBucket))).SingleInstance();
        builder.Register(c => new RoadNetworkJobsBlobClient(c.Resolve<IBlobClientFactory>().Create(WellKnownBuckets.JobsBucket))).SingleInstance();
    }
}
