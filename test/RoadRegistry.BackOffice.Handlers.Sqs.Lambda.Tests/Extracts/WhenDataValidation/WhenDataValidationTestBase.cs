namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using Actions.DataValidation;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadNetwork;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public abstract class WhenDataValidationTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }
    protected Mock<IDataValidationApiClient> DataValidationApiClientMock { get; } = new();

    protected WhenDataValidationTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
    }

    protected async Task<DataValidationSqsRequest> HandleRequest(
        MigrateRoadNetworkSqsRequest? migrateRoadNetworkSqsRequest = null,
        TicketId? ticketId = null,
        bool featureEnabled = false)
    {
        var sqsRequest = new DataValidationSqsRequest
        {
            TicketId = ticketId ?? Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            MigrateRoadNetworkSqsRequest = migrateRoadNetworkSqsRequest ?? ObjectProvider.Create<MigrateRoadNetworkSqsRequest>()
        };

        var sqsLambdaRequest = new DataValidationSqsLambdaRequest(sqsRequest.MigrateRoadNetworkSqsRequest.DownloadId.ToString(), sqsRequest);

        var blobClientMock = new Mock<IBlobClient>();
        blobClientMock
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlobObject(
                new BlobName("abc"),
                Metadata.None,
                ContentType.Parse("X-multipart/abc"),
                _ => Task.FromResult<Stream>(new MemoryStream())));

        var sqsLambdaRequestHandler = new DataValidationSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            DataValidationApiClientMock.Object,
            ExtractsDbContext,
            new SqsJsonMessageSerializer(new FakeSqsOptions(), SqsJsonMessageAssemblies.Assemblies),
            new RoadNetworkUploadsBlobClient(blobClientMock.Object),
            new UseDataValidationFeatureToggle(featureEnabled),
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }
}
