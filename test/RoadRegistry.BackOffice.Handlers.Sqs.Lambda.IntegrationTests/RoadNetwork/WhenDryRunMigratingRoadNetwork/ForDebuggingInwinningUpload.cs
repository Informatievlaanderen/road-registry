namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenDryRunMigratingRoadNetwork;

using System.Data;
using System.IO.Compression;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.Inwinning;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.Inwinning;
using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.Problems;
using Xunit.Abstractions;

// TEMPORARY - diagnostics for the inwinning upload that reports a missing start/end road node and a removed road
// segment after a neighbouring inwinning removed both (follow-up on GAWR-7827).
//
// Reads an upload zip from disk, runs the inwinning feature compare on it and replays the migration against the road
// network as it is on the server the local configuration points at. Nothing is written: the Marten session is never
// saved, the extracts database is only read, and no ticket or e-mail goes out. The point of the run is the DIAG lines
// that ScopedRoadNetwork-Diagnostics.cs writes while the changes are applied.
public class ForDebuggingInwinningUpload
{
    private readonly ITestOutputHelper _outputHelper;

    public ForDebuggingInwinningUpload(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task DryRunUploadOnActualServer()
    {
        // Arrange
        var zipPath = @"C:\Users\RikDePeuter\Downloads\ab4c86506ce94d6e8ef30bf56b71ff5c_upload_202611191446.zip";
        var downloadId = DownloadId.Parse("ab4c86506ce94d6e8ef30bf56b71ff5c");

        var sp = BuildServiceProvider();

        using var loggerFactory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Information)
            .AddProvider(new TestOutputLoggerProvider(_outputHelper)));
        var logger = loggerFactory.CreateLogger<ForDebuggingInwinningUpload>();

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var inwinningszone = extractsDbContext.Inwinningszones
            .Single(x => x.DownloadId == downloadId.ToGuid());
        logger.LogInformation("Inwinningszone niscode {NisCode}, operator {Operator}, completed {Completed}",
            inwinningszone.NisCode, inwinningszone.Operator, inwinningszone.Completed);

        var provenanceData = new RoadRegistryProvenanceData(operatorName: inwinningszone.Operator);

        // Feature compare, the same way UploadInwinningExtractSqsLambdaRequestHandler runs it.
        TranslatedChanges translatedChanges;
        await using (var fileStream = File.OpenRead(zipPath))
        {
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(GrbOgcApiFeaturesDownloader));
            var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
                grbOgcApiFeaturesDownloader: new GrbOgcApiFeaturesDownloader(client, "https://geo.api.vlaanderen.be/GRB/ogc/features/v1"));

            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            translatedChanges = await translator.TranslateAsync(archive, ZipArchiveMetadata.Empty.WithDownloadId(downloadId), CancellationToken.None);
        }

        var roadNetworkChanges = translatedChanges
            .Select(ChangeRoadNetworkItem.Create)
            .ToList()
            .ToRoadNetworkChanges(provenanceData);
        logger.LogInformation("Feature compare produced {Count} change(s)", roadNetworkChanges.Count);

        // Loaded the same way MigrateDryRunRoadNetworkSqsLambdaRequestHandler loads it.
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession(IsolationLevel.Snapshot);

        var roadNetwork = await new RoadNetworkRepository(store).LoadUnderlying(
            session,
            new ScopedRoadNetworkId(downloadId.ToGuid()),
            roadNetworkChanges.BuildScopeGeometry(),
            roadNetworkChanges.Ids);
        logger.LogInformation("Loaded {RoadNodeCount} road node(s) and {RoadSegmentCount} road segment(s) in scope",
            roadNetwork.RoadNodes.Count, roadNetwork.RoadSegments.Count);

        // Act - the session is never saved, so this only produces events in memory plus the DIAG logging we are after.
        var changeResult = roadNetwork.Migrate(
            roadNetworkChanges,
            downloadId,
            new InMemoryRoadNetworkIdGenerator(initialValue: 1_000_000_000),
            logger);

        // Assert - printed rather than asserted: this test is here to be read, not to pass or fail. The Dutch text is
        // left out on purpose; the reason plus its parameters is what points back into the code.
        foreach (var problem in changeResult.Problems)
        {
            logger.LogInformation("PROBLEM {Kind} {Reason} [{Parameters}]",
                problem is Error ? "Error" : "Warning",
                problem.Reason,
                string.Join(", ", problem.Parameters.Select(x => $"{x.Name}={x.Value}")));
        }

        logger.LogInformation("{Count} problem(s), of which {ErrorCount} error(s)",
            changeResult.Problems.Count(),
            changeResult.Problems.Count(x => x is Error));
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddHttpClient()
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>());

        services
            .AddMartenRoad(options => options
                .AddRoadAggregatesSnapshots()
                .ConfigureExtractDocuments()).Services
            .AddSingleton<IRoadNetworkIdGenerator>(new InMemoryRoadNetworkIdGenerator())
            .AddExtractsDbContext(QueryTrackingBehavior.NoTracking);

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    private sealed class TestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestOutputLoggerProvider(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(_outputHelper);
        }

        public void Dispose()
        {
        }

        private sealed class TestOutputLogger : ILogger
        {
            private readonly ITestOutputHelper _outputHelper;

            public TestOutputLogger(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                try
                {
                    _outputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
                    if (exception is not null)
                    {
                        _outputHelper.WriteLine(exception.ToString());
                    }
                }
                catch
                {
                    // the output helper throws once the test is over, and there is nothing to be done about it here
                }
            }
        }
    }
}
