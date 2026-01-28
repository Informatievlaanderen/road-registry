namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestInwinningExtract;

using Actions.RequestInwinningExtract;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public abstract class WhenRequestInwinningExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }

    protected WhenRequestInwinningExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
    }

    protected async Task<RequestInwinningExtractSqsRequest> HandleRequest(
        RequestExtractData request,
        string? nisCode = null,
        IBlobClient? blobClient = null,
        IRoadNetworkExtractArchiveAssembler? archiveAssembler = null,
        TicketId? ticketId = null)
    {
        var sqsRequest = new RequestInwinningExtractSqsRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = request.DownloadId,
            Contour = request.Contour.ToExtractGeometry(),
            Description = request.Description,
            IsInformative = request.IsInformative,
            TicketId = ticketId ?? Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            NisCode = nisCode ?? ObjectProvider.Create<string>()
        };

        var sqsLambdaRequest = new RequestInwinningExtractSqsLambdaRequest(sqsRequest.DownloadId.ToString(), sqsRequest);

        var archiveAssemblerMock = new Mock<IRoadNetworkExtractArchiveAssembler>();
        archiveAssemblerMock
            .Setup(x => x.AssembleArchive(It.IsAny<RoadNetworkExtractAssemblyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        var sqsLambdaRequestHandler = new RequestInwinningExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ExtractsDbContext,
            new ExtractRequester(
                ExtractsDbContext,
                new RoadNetworkExtractDownloadsBlobClient(blobClient ?? Mock.Of<IBlobClient>()),
                archiveAssembler ?? archiveAssemblerMock.Object,
                new NullLoggerFactory()),
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }
}
