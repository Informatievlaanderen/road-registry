namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenCloseExtract;

using Abstractions.Extracts.V2;
using Actions.CloseExtract;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.Extracts;
using Xunit.Abstractions;

public abstract class WhenCloseExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }

    protected WhenCloseExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
    }

    protected async Task HandleRequest(CloseExtractSqsRequest sqsRequest)
    {
        sqsRequest.TicketId = Guid.NewGuid();
        sqsRequest.Metadata = new Dictionary<string, object?>();
        sqsRequest.ProvenanceData = ObjectProvider.Create<ProvenanceData>();

        var sqsLambdaRequest = new CloseExtractSqsLambdaRequest(sqsRequest.DownloadId, sqsRequest);

        var sqsLambdaRequestHandler = new CloseExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ExtractsDbContext,
            new NullLogger<CloseExtractSqsLambdaRequestHandler>()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);
    }
}
