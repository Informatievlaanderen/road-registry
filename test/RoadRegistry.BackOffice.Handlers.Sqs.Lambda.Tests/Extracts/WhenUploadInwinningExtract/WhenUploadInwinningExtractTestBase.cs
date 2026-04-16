namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using System.IO.Compression;
using Actions.UploadExtract;
using Actions.UploadInwinningExtract;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Framework;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Sqs.Extracts;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public abstract class WhenUploadInwinningExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }
    protected Mock<IMediator> MediatorMock { get; }
    protected IFixture Fixture { get; }

    protected WhenUploadInwinningExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        MediatorMock = new Mock<IMediator>();
        Fixture = new RoadNetworkTestDataV2().Fixture;
    }

    protected async Task<UploadInwinningExtractSqsRequest> HandleRequest(
        UploadInwinningExtractSqsRequest sqsRequest,
        IExtractUploader? extractUploader = null)
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
            extractUploader ?? new FakeExtractUploader(),
            new TransactionZoneFeatureCompareFeatureReader(FileEncoding),
            new RoadNodeFeatureCompareFeatureReader(FileEncoding),
            MediatorMock.Object,
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }

    private sealed class FakeExtractUploader : IExtractUploader
    {
        public Task<TranslatedChanges> ProcessUploadAndDetectChanges(DownloadId downloadId, UploadId uploadId, TicketId ticketId, ZipArchiveMetadata zipArchiveMetadata, bool sendFailedEmail, Func<ZipArchive, Task>? beforeFeatureCompare = null, Func<ZipArchive, TranslatedChanges, Task>? afterFeatureCompare = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TranslatedChanges.Empty);
        }
    }
}
