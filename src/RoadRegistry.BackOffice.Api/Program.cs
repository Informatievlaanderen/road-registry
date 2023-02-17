namespace RoadRegistry.BackOffice.Api;

using Abstractions;
using Amazon;
using Amazon.DynamoDBv2;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Editor.Schema;
using Hosts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Product.Schema;
using SqlStreamStore;
using Syndication.Schema;
using System;
using System.Text;
using System.Threading.Tasks;
using Abstractions.Configuration;
using BackOffice.Configuration;
using ZipArchiveWriters.Validation;

public class Program
{
    public const int HostingPort = 10002;

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder= new RoadRegistryHostBuilder<Program>(args)
            .ConfigureWebHostDefaults(webHostBuilder => webHostBuilder
                .UseDefaultForApi<Startup>(
                    new ProgramOptions
                    {
                        Hosting = { HttpPort = HostingPort },
                        Logging =
                        {
                            WriteTextToConsole = false,
                            WriteJsonToConsole = false
                        },
                        Runtime = { CommandLineArgs = args }
                    })
                .UseKestrel((context, builder) =>
                {
                    if (context.HostingEnvironment.EnvironmentName == "Development")
                    {
                        builder.ListenLocalhost(HostingPort);
                    }
                })
            ) as RoadRegistryHostBuilder<Program>;

        var roadRegistryHost = builder
            .ConfigureOptions<ZipArchiveWriterOptions>(out var zipArchiveWriterOptions)
            .ConfigureOptions<ExtractDownloadsOptions>(out var extractDownloadsOptions)
            .ConfigureOptions<ExtractUploadsOptions>(out var extractUploadsOptions)
            .ConfigureOptions<FeatureCompareMessagingOptions>(out var featureCompareMessagingOptions)
            .ConfigureOptions<SqsQueueUrlOptions>(out var sqsQueueUrlOptions)
            .ConfigureServices((hostContext, services) => services
                    .AddSingleton(new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
                    .AddSingleton<IZipArchiveBeforeFeatureCompareValidator>(new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<IZipArchiveAfterFeatureCompareValidator>(new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<ProblemDetailsHelper>()
                    .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
                    .AddScoped(sp => new TraceDbConnection<EditorContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.EditorProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                    .AddScoped(sp => new TraceDbConnection<SyndicationContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                    .AddSingleton<IStreetNameCache, StreetNameCache>()
                    .AddSingleton<Func<SyndicationContext>>(sp =>
                        () =>
                            new SyndicationContext(
                                new DbContextOptionsBuilder<SyndicationContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                                    .UseSqlServer(
                                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                        options => options
                                            .EnableRetryOnFailure()
                                    )
                                    .Options)
                    )
                    .AddDbContext<EditorContext>((sp, options) => options
                        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .UseSqlServer(
                            sp.GetRequiredService<TraceDbConnection<EditorContext>>(),
                            sqlOptions => sqlOptions
                                .UseNetTopologySuite())
                    )
                    .AddScoped(sp => new TraceDbConnection<ProductContext>(
                        new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.ProductProjections)),
                        sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                    .AddDbContext<ProductContext>((sp, options) => options
                        .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .UseSqlServer(
                            sp.GetRequiredService<TraceDbConnection<ProductContext>>())))
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IRoadNetworkSnapshotWriter>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }))
        .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.Snapshots,
                WellknownConnectionNames.EditorProjections,
                WellknownConnectionNames.ProductProjections,
                WellknownConnectionNames.SyndicationProjections
            })
            .RunAsync();
    }
}
