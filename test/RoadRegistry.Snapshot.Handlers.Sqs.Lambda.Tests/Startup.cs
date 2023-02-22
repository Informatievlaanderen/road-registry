namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using MediatorModule = Sqs.MediatorModule;

public class Startup : TestStartup
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

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
        var eventSourcedEntityMap = new EventSourcedEntityMap();

        var minioServer = hostBuilderContext.Configuration.GetValue<string>("MINIO_SERVER");

        var s3Client = new AmazonS3Client(
            new BasicAWSCredentials(
                hostBuilderContext.Configuration.GetRequiredValue<string>("MINIO_ACCESS_KEY"),
                hostBuilderContext.Configuration.GetRequiredValue<string>("MINIO_SECRET_KEY")
            ),
            new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                ServiceURL = minioServer,
                ForcePathStyle = true
            }
        );

        services.AddSingleton(s3Client);

        services
            .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => eventSourcedEntityMap)
            .AddTransient<ICustomRetryPolicy>(sp => new FakeRetryPolicy())
            .AddRoadRegistrySnapshot()
            .RegisterDistributedS3Cache(s3Client, new()
            {
                Bucket = "road-registry-snapshots",
                RootDir = "snapshots"
            })
            ;
    }
}
