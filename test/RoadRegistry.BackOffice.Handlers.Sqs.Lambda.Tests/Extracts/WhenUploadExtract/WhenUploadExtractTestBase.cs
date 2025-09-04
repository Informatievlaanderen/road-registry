namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadExtract;

using System.IO.Compression;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FeatureCompare;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using SqlStreamStore;
using Xunit.Abstractions;
using ZipArchiveWriters;
using ZipArchiveWriters.Cleaning;
using Reason = BackOffice.Reason;

public abstract class WhenUploadExtractTestBase : BackOfficeLambdaTest
{
    protected ExtractsDbContext ExtractsDbContext { get; }
    protected IStreamStore StreamStore { get; }

    protected WhenUploadExtractTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        StreamStore = new InMemoryStreamStore();
    }

    protected async Task<UploadExtractSqsRequest> HandleRequest(
        UploadExtractRequest request,
        IBlobClient? blobClient = null,
        IExtractUploadFailedEmailClient? extractUploadFailedEmailClient = null)
    {
        var sqsRequest = new UploadExtractSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new UploadExtractSqsLambdaRequest(new DownloadId(request.DownloadId), sqsRequest);

        var featureCompareValidatorFactory = new FakeZipArchiveBeforeFeatureCompareValidatorFactory(() => new FakeZipArchiveBeforeFeatureCompareValidator());

        if (blobClient is null)
        {
            var blobClientMock = new Mock<IBlobClient>();
            blobClientMock
                .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(new BlobName("archive.zip"),
                    Metadata.None,
                    ContentType.Parse("application/zip"),
                    _ =>
                    {
                        var archiveStream = new ExtractsZipArchiveBuilder()
                            .BuildArchiveStream();
                        return Task.FromResult<Stream>(archiveStream);
                    }));
            blobClient = blobClientMock.Object;
        }

        var sqsLambdaRequestHandler = new UploadExtractSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ExtractsDbContext,
            new RoadNetworkUploadsBlobClient(blobClient),
            featureCompareValidatorFactory,
            StreamStore,
            new FakeBeforeFeatureCompareZipArchiveCleanerFactory(),
            new FakeZipArchiveFeatureCompareTranslatorFactory(),
            extractUploadFailedEmailClient ?? new FakeExtractUploadFailedEmailClient(),
            new NullLoggerFactory()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return sqsRequest;
    }

    private sealed class FakeZipArchiveBeforeFeatureCompareValidatorFactory : IZipArchiveBeforeFeatureCompareValidatorFactory
    {
        private readonly Func<IZipArchiveBeforeFeatureCompareValidator> _builder;

        public FakeZipArchiveBeforeFeatureCompareValidatorFactory(Func<IZipArchiveBeforeFeatureCompareValidator> builder)
        {
            _builder = builder;
        }

        public IZipArchiveBeforeFeatureCompareValidator Create(string zipArchiveWriterVersion)
        {
            return _builder();
        }
    }

    private sealed class FakeZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
    {
        public Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
        {
            return Task.FromResult(ZipArchiveProblems.None);
        }
    }

    private sealed class FakeBeforeFeatureCompareZipArchiveCleanerFactory : IBeforeFeatureCompareZipArchiveCleanerFactory
    {
        public IBeforeFeatureCompareZipArchiveCleaner Create(string zipArchiveWriterVersion)
        {
            return new FakeBeforeFeatureCompareZipArchiveCleaner();
        }
    }

    private sealed class FakeBeforeFeatureCompareZipArchiveCleaner : IBeforeFeatureCompareZipArchiveCleaner
    {
        public Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken)
        {
            return Task.FromResult(CleanResult.NoChanges);
        }
    }

    private sealed class FakeZipArchiveFeatureCompareTranslatorFactory : IZipArchiveFeatureCompareTranslatorFactory
    {
        public IZipArchiveFeatureCompareTranslator Create(string zipArchiveWriterVersion)
        {
            return new FakeZipArchiveFeatureCompareTranslator();
        }
    }

    private sealed class FakeZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
    {
        public Task<TranslatedChanges> TranslateAsync(ZipArchive archive, CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedChanges.Empty.WithReason(new Reason("reason")));
        }
    }
}
