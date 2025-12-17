namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestExtract;

using Actions.RequestExtract;
using AutoFixture;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FeatureToggles;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public abstract class WhenRequestExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }

    protected WhenRequestExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
    }

    protected async Task<RequestExtractSqsRequest> HandleRequest(
        RequestExtractData request,
        IBlobClient? blobClient = null,
        IRoadNetworkExtractArchiveAssembler? archiveAssembler = null,
        TicketId? ticketId = null)
    {
        var sqsRequest = new RequestExtractSqsRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = request.DownloadId,
            Contour = request.Contour,
            Description = request.Description,
            IsInformative = request.IsInformative,
            ExternalRequestId = request.ExternalRequestId,
            TicketId = ticketId ?? Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
        };

        var sqsLambdaRequest = new RequestExtractSqsLambdaRequest(sqsRequest.DownloadId, sqsRequest);

        var archiveAssemblerMock = new Mock<IRoadNetworkExtractArchiveAssembler>();
        archiveAssemblerMock
            .Setup(x => x.AssembleArchive(It.IsAny<RoadNetworkExtractAssemblyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        var sqsLambdaRequestHandler = new RequestExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            new ExtractRequester(
                ExtractsDbContext,
                new RoadNetworkExtractDownloadsBlobClient(blobClient ?? Mock.Of<IBlobClient>()),
                archiveAssembler ?? archiveAssemblerMock.Object,
                new UseDomainV2FeatureToggle(true),
                new NullLoggerFactory()),
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }
}
