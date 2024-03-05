namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using MediatorModule = Sqs.MediatorModule;

public class Startup : TestStartup
{
    private void AddS3ClientAndDistributedCache(IServiceCollection services, IConfiguration configuration)
    {
        var s3Configuration = configuration.GetOptions<S3Options>();
        var s3OptionsJsonSerializer = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        S3Options s3ClientOptions;
        if (s3Configuration?.ServiceUrl is not null)
        {
            var developments3Configuration = configuration.GetOptions<DevelopmentS3Options>();
            s3ClientOptions = new DevelopmentS3Options(s3OptionsJsonSerializer, developments3Configuration);
        }
        else
        {
            s3ClientOptions = new S3Options(s3OptionsJsonSerializer);
        }

        var s3Client = s3ClientOptions.CreateS3Client();

        services
            .AddSingleton(s3ClientOptions)
            .AddSingleton(s3Client)
            .RegisterDistributedS3Cache(s3Client, new DistributedS3CacheOptions
            {
                Bucket = "road-registry-snapshots",
                RootDir = "snapshots"
            });
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        builder.Register<SqsLambdaHandlerOptions>(c => new FakeSqsLambdaHandlerOptions());
        builder.Register<IRoadNetworkCommandQueue>(c => new RoadNetworkCommandQueue(c.Resolve<IStreamStore>(), new ApplicationMetadata(RoadRegistryApplication.Lambda)));

        builder
            .RegisterModule<BackOffice.MediatorModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<BackOffice.Handlers.MediatorModule>()
            .RegisterModule<MediatorModule>()
            ;
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var configuration = hostBuilderContext.Configuration;

        AddS3ClientAndDistributedCache(services, configuration);

        services
            .AddSingleton<EventSourcedEntityMap>(_ => new EventSourcedEntityMap())
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            .AddRoadRegistrySnapshot()
            .AddSingleton<IRoadNetworkSnapshotReader, FakeRoadNetworkSnapshotReader>()
            .AddSingleton<IRoadNetworkSnapshotWriter, FakeRoadNetworkSnapshotWriter>()
            ;
    }
}
