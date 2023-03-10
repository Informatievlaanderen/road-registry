namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
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

        var configuration = hostBuilderContext.Configuration;
        var s3Options = configuration.GetOptions<S3Options>();
        var s3ClientOptions = s3Options?.ServiceUrl is not null
            ? new DevelopmentS3Options(EventsJsonSerializerSettingsProvider.CreateSerializerSettings(), s3Options.ServiceUrl)
            : new S3Options(EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
        var s3Client = s3ClientOptions.CreateS3Client();

        services.AddSingleton<S3Options>(s3ClientOptions);
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
