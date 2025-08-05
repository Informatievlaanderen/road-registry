using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Extensions;
using BackOffice.Infrastructure.Modules;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
using FluentValidation;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StreetName;
using System.Reflection;
using Abstractions;
using BackOffice.Extracts;
using Editor.Schema;
using Handlers.Extracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.Extracts.Schema;
using ZipArchiveWriters.ExtractHost;

public class Function : RoadRegistryLambdaFunction<MessageHandler>
{
    protected override string ApplicationName => "RoadRegistry.BackOffice.Handlers.Sqs.Lambda";

    public Function()
        : base(new List<Assembly>
            {
                typeof(BackOffice.Handlers.Sqs.DomainAssemblyMarker).Assembly,
                typeof(RoadRegistry.BackOffice.Abstractions.BlobRequest).Assembly
            })
    {
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        //TODO-pr add ExtractsDbContext
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
            .AddValidatorsFromAssemblyContaining<BackOffice.DomainAssemblyMarker>()
            .AddStreetNameClient()

            // Extracts
            .AddDbContext<ExtractsDbContext>((sp, options) =>
            {
                options
                    .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .UseSqlServer(
                        sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.Extracts),
                        sqlOptions => ExtractsDbContext.ConfigureSqlServerOptions(sqlOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(ExtractsDbContextMigratorFactory.Configuration.Table, ExtractsDbContextMigratorFactory.Configuration.Schema)));
            })
            .AddEditorContext()
            .RegisterOptions<ZipArchiveWriterOptions>()
            .AddSingleton<IZipArchiveWriterFactory>(sp =>
                new ZipArchiveWriterFactory(
                    null,
                    new ZipArchiveWriters.ExtractHost.V2.RoadNetworkExtractZipArchiveWriter(
                        sp.GetService<ZipArchiveWriterOptions>(),
                        sp.GetService<IStreetNameCache>(),
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetRequiredService<FileEncoding>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    ))
            )
            .AddSingleton<IRoadNetworkExtractArchiveAssembler>(sp =>
                new RoadNetworkExtractArchiveAssembler(
                    sp.GetService<RecyclableMemoryStreamManager>(),
                    sp.GetService<Func<EditorContext>>(),
                    sp.GetService<IZipArchiveWriterFactory>()))
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
