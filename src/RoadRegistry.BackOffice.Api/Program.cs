namespace RoadRegistry.BackOffice.Api;

using Abstractions;
using Abstractions.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using BackOffice.Configuration;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Editor.Schema;
using Extensions;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Product.Schema;
using Serilog;
using SqlStreamStore;
using Syndication.Schema;
using System;
using System.Text;
using System.Threading.Tasks;
using ZipArchiveWriters.Validation;

public class Program
{
    public const int HostingPort = 10002;

    protected Program()
    {
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        var webHostBuilder = new WebHostBuilder()
            .UseDefaultForApi<Startup>(
                new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = HostingPort
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    }
                })
            .UseKestrel((context, builder) =>
            {
                if (context.HostingEnvironment.EnvironmentName == "Development")
                {
                    builder.ListenLocalhost(HostingPort);
                }
            })
            .ConfigureServices((hostContext, builder) =>
            {
                var zipArchiveWriterOptions = new ZipArchiveWriterOptions();
                hostContext.Configuration.GetSection(nameof(ZipArchiveWriterOptions)).Bind(zipArchiveWriterOptions);
                var extractDownloadsOptions = new ExtractDownloadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractDownloadsOptions)).Bind(extractDownloadsOptions);
                var extractUploadsOptions = new ExtractUploadsOptions();
                hostContext.Configuration.GetSection(nameof(ExtractUploadsOptions)).Bind(extractDownloadsOptions);

                var featureCompareMessagingOptions = new FeatureCompareMessagingOptions();
                hostContext.Configuration.GetSection(FeatureCompareMessagingOptions.ConfigurationKey).Bind(featureCompareMessagingOptions);

                var sqsQueueUrlOptions = new SqsQueueUrlOptions();
                hostContext.Configuration.Bind(sqsQueueUrlOptions);

                builder
                    .AddSingleton(new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
                    .AddSingleton<IZipArchiveBeforeFeatureCompareValidator>(new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<IZipArchiveAfterFeatureCompareValidator>(new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
                    .AddSingleton<ProblemDetailsHelper>()
                    .AddSingleton(zipArchiveWriterOptions)
                    .AddSingleton(extractDownloadsOptions)
                    .AddSingleton(extractUploadsOptions)
                    .AddSingleton(featureCompareMessagingOptions)
                    .AddSingleton(sqsQueueUrlOptions)
                    .AddStreamStore()
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSingleton(new WKTReader(
                        new NtsGeometryServices(
                            GeometryConfiguration.GeometryFactory.PrecisionModel,
                            GeometryConfiguration.GeometryFactory.SRID
                        )
                    ))
                    .AddRoadRegistrySnapshot()
                    .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                        new CommandHandlerModule[]
                        {
                            new RoadNetworkChangesArchiveCommandModule(sp.GetService<RoadNetworkUploadsBlobClient>(),
                                sp.GetService<IStreamStore>(),
                                sp.GetService<Func<EventSourcedEntityMap>>(),
                                sp.GetService<IRoadNetworkSnapshotReader>(),
                                sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
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
                                sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                                sp.GetService<IClock>(),
                                sp.GetService<ILoggerFactory>()
                            )
                        })))
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
                            sp.GetRequiredService<TraceDbConnection<ProductContext>>()));
            });
        return webHostBuilder;
    }

    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var streamStore = host.Services.GetRequiredService<IStreamStore>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        try
        {
            await WaitFor.SeqToBecomeAvailable(configuration).ConfigureAwait(false);
            await WaitFor.SqlStreamStoreToBecomeAvailable(streamStore, logger).ConfigureAwait(false);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Events);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.Snapshots);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.EditorProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.ProductProjections);
            logger.LogSqlServerConnectionString(configuration, WellknownConnectionNames.SyndicationProjections);

            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
