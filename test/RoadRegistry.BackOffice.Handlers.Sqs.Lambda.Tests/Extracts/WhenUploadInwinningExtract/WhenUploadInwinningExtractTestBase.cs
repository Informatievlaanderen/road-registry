namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using Actions.UploadExtract;
using Actions.UploadInwinningExtract;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.Extracts;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public abstract class WhenUploadInwinningExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }
    protected Mock<IMediator> MediatorMock { get; }

    protected WhenUploadInwinningExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        MediatorMock = new Mock<IMediator>();
    }

    protected async Task<UploadInwinningExtractSqsRequest> HandleRequest(
        UploadInwinningExtractSqsRequest sqsRequest)
    {
        sqsRequest.TicketId = Guid.NewGuid();
        sqsRequest.Metadata = new Dictionary<string, object?>();
        sqsRequest.ProvenanceData = ObjectProvider.Create<ProvenanceData>();

        var sqsLambdaRequest = new UploadInwinningExtractSqsLambdaRequest(sqsRequest.DownloadId.ToString(), sqsRequest);

        var sqsLambdaRequestHandler = new UploadInwinningExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ExtractsDbContext,
            new FakeExtractUploader(),
            MediatorMock.Object,
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }

    private sealed class FakeExtractUploader : IExtractUploader
    {
        public Task<TranslatedChanges> ProcessUploadAndDetectChanges(DownloadId downloadId, UploadId uploadId, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedChanges.Empty);
        }
    }
}
