namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestExtract;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class ForDebugging
{
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebugging(ITestOutputHelper outputHelper)
    {
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task DomainV1Extract_WithCustomRequestOnActualServer()
    {
        // Arrange
        var downloadId = DownloadId.Parse("e58c1a02e5884ec7a66ba28860d35fc3");
        var @operator = "OVO002949";

        var sp = BuildServiceProvider();

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();

        var request = new RequestExtractData(
            ExtractRequestId.FromString("5c477d4a13327dfed70de198ecf927d2d4cbd0e29a90bec10c34c16d762d3751"),
            downloadId,
            (MultiPolygon)new WKTReader().Read("MULTIPOLYGON (((91882.398321789413 191674.04542286764, 91876.896012585043 191692.19976078474, 91874.921030602869 191711.06652621919, 91876.544446634391 191729.96678844429, 91881.7078411784 191748.22041133323, 91890.225406698242 191765.17052842441, 91901.790634006189 191780.20718061511, 91915.987342161854 191792.78926586884, 91932.304654967651 191802.46401106374, 92464.240375116147 192053.01898651064, 92483.272235377968 192059.73245811393, 92503.26447500482 192062.48795456591, 92523.402845251025 192061.17324934408, 92991.624901210133 191982.59685897251, 93011.33915901721 191977.16861571081, 93029.537125856572 191967.84382141248, 93045.457912368132 191955.01236231206, 93058.435842151463 191939.21074453631, 93067.928284880065 191921.09966182883, 93073.538344640358 191901.43637076966, 93075.031454855853 191881.04302853066, 93072.345185932834 191860.77231702203, 93065.591855552688 191841.47179074504, 92846.2979843336 191374.46087830648, 92836.432827650817 191357.84466475074, 92823.555956652155 191343.43612344391, 92808.148219286144 191331.77329794725, 92790.784971080153 191323.29170133793, 92772.114590221739 191318.30805328576, 92154.239287608783 191216.01051035905, 92135.090397749547 191214.7071224823, 92116.0452029396 191217.08606288696, 92097.805287785217 191223.05969645118, 92081.0425720972 191232.40796717687, 92066.374558789234 191244.78650458626, 92054.34158637948 191259.73930960018, 92045.38692406498 191276.71555257769, 91882.398321789413 191674.04542286764)))"),
            "On-demand test",
            false,
            null
        );
        var provenanceData = new RoadRegistryProvenanceData(operatorName: @operator);

        // Act
        var sqsRequest = new RequestExtractSqsRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = request.DownloadId,
            Contour = request.Contour.ToExtractGeometry(),
            Description = request.Description,
            IsInformative = request.IsInformative,
            ExternalRequestId = null,
            ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV1_2,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = provenanceData
        };

        var sqsLambdaRequest = new RequestExtractSqsLambdaRequest(sqsRequest.DownloadId.ToString(), sqsRequest);

        var sqsLambdaRequestHandler = new RequestExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            sp.GetRequiredService<IRoadRegistryContext>(),
            new ExtractRequester(
                extractsDbContext,
                new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
                new RoadNetworkExtractArchiveAssembler(sp),
                new NullLoggerFactory()),
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(TicketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>())
            .AddSingleton(new ZipArchiveWriterOptions())
            .AddSingleton(new RecyclableMemoryStreamManager())
            .AddSingleton(FileEncoding.UTF8)

            // Extracts-domainv1
            .AddStreamStore()
            .AddEditorContext()
            .AddSingleton<IStreetNameCache>(new FakeStreetNameCache())
            .AddSingleton<IZipArchiveWriterFactory>(sp =>
                new ZipArchiveWriterFactory(
                    null,
                    new RoadNetworkExtractZipArchiveWriter(
                        sp.GetService<ZipArchiveWriterOptions>(),
                        sp.GetService<IStreetNameCache>(),
                        sp.GetService<RecyclableMemoryStreamManager>(),
                        sp.GetRequiredService<FileEncoding>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    )
                ))

            .AddScoped<RoadNetworkExtractArchiveAssemblerForDomainV1>()
            ;

        services
            //.AddMartenRoad(options => { options.ConfigureExtractDocuments(); }).Services
            //.AddSingleton<IRoadNetworkIdGenerator>(new InMemoryRoadNetworkIdGenerator())
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            .AddEditorContext();

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
