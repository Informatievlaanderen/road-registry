[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using System.Reflection;
using Abstractions;
using Actions.RequestExtract;
using Actions.UploadExtract;
using Autofac;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Handlers.Extracts;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using CommandHandling;
using Editor.Schema;
using FeatureToggles;
using FluentValidation;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;
using SqlStreamStore;
using StreetName;
using ZipArchiveWriters.Cleaning;
using ZipArchiveWriters.ExtractHost.V2;
using DomainAssemblyMarker = BackOffice.DomainAssemblyMarker;

public class Function : RoadRegistryLambdaFunction<MessageHandler>
{
    protected override string ApplicationName => "RoadRegistry.BackOffice.Handlers.Sqs.Lambda";

    public Function()
        : base(new List<Assembly>
            {
                typeof(DomainAssemblyMarker).Assembly,
                typeof(BlobRequest).Assembly
            })
    {
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services
            .AddStreetNameCache()
            .AddDistributedStreamStoreLockOptions()
            .AddRoadNetworkCommandQueue()
            .AddRoadNetworkEventWriter()
            .AddChangeRoadNetworkDispatcher()
            .AddRoadNetworkDbIdGenerator()
            .AddCommandHandlerDispatcher(sp => Resolve.WhenEqualToMessage(
                [
                    CommandModules.RoadNetwork(sp)
                ])
            )
            .AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>()
            .AddStreetNameClient()

            // ChangeRoadNetwork
            .AddMartenRoad(options => options
                .AddRoadNetworkTopologyProjection()
                .AddRoadAggregatesSnapshots())

            // Extracts-domainv1
            .AddEditorContext()
            .AddSingleton<IBeforeFeatureCompareZipArchiveCleanerFactory, BeforeFeatureCompareZipArchiveCleanerFactory>()
            .AddSingleton<ZipArchiveWriters.ExtractHost.IZipArchiveWriterFactory>(sp =>
                new ZipArchiveWriters.ExtractHost.ZipArchiveWriterFactory(
                    null,
                    new RoadNetworkExtractZipArchiveWriter(
                        sp.GetService<ZipArchiveWriterOptions>(),
                        sp.GetService<IStreetNameCache>(),
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetRequiredService<FileEncoding>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    )
                ))
            // Extracts-domainv2
            .AddMartenRoad()
            .AddScoped<ZipArchiveWriters.DomainV2.IZipArchiveWriterFactory>(sp =>
                new ZipArchiveWriters.DomainV2.ZipArchiveWriterFactory(
                    new ZipArchiveWriters.DomainV2.Writers.RoadNetworkExtractZipArchiveWriter(
                        sp.GetService<ZipArchiveWriterOptions>(),
                        sp.GetService<IStreetNameCache>(),
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetRequiredService<FileEncoding>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    )
                ))

            // Extracts
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            .AddScoped<IExtractRequests, ExtractRequests>()
            .AddScoped<ExtractRequester>()
            .AddScoped<ExtractUploader>()
            .RegisterOptions<ZipArchiveWriterOptions>()
            .AddScoped<IRoadNetworkExtractArchiveAssembler>(sp =>
                sp.GetService<UseDomainV2FeatureToggle>().FeatureEnabled
                    ? new RoadNetworkExtractArchiveAssemblerForDomainV2(
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetService<RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2.IZipArchiveWriterFactory>(),
                        sp.GetRequiredService<IDocumentStore>(),
                        sp.GetRequiredService<IRoadNetworkRepository>(),
                        sp.GetService<Func<EditorContext>>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    )
                    : new RoadNetworkExtractArchiveAssemblerForDomainV1(
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetService<Func<EditorContext>>(),
                        sp.GetService<RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.IZipArchiveWriterFactory>(),
                        sp.GetRequiredService<IStreamStore>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    ))

            .AddFeatureCompare()
            ;
    }

    protected override void ConfigureContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        builder
            .RegisterModule(new EventHandlingModule(typeof(BackOffice.Handlers.DomainAssemblyMarker).Assembly, EventSerializerSettings))
            .RegisterModule<CommandHandlingModule>()
            .RegisterModule<ContextModule>()
            .RegisterModule<SqsHandlersModule>()
            ;

        builder.RegisterIdempotentCommandHandler();
    }
}
