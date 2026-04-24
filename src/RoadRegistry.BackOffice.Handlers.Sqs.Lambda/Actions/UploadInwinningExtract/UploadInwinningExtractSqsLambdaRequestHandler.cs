namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadInwinningExtract;

using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;
using UploadExtract;

public sealed class UploadInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<UploadInwinningExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IExtractUploader _extractUploader;
    private readonly TransactionZoneFeatureCompareFeatureReader _transactionZoneFeatureCompareFeatureReader;
    private readonly RoadNodeFeatureCompareFeatureReader _roadNodeFeatureCompareFeatureReader;
    private readonly IMediator _mediator;

    public UploadInwinningExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        IExtractUploader extractUploader,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureCompareFeatureReader,
        RoadNodeFeatureCompareFeatureReader roadNodeFeatureCompareFeatureReader,
        IMediator mediator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<UploadExtractSqsLambdaRequestV2Handler>(),
            TicketingBehavior.Error)
    {
        _extractsDbContext = extractsDbContext;
        _extractUploader = extractUploader;
        _transactionZoneFeatureCompareFeatureReader = transactionZoneFeatureCompareFeatureReader;
        _roadNodeFeatureCompareFeatureReader = roadNodeFeatureCompareFeatureReader;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(UploadInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var inwinningszone = await _extractsDbContext.Inwinningszones.SingleOrDefaultAsync(x => x.DownloadId == request.Request.DownloadId.ToGuid(), cancellationToken);
            if (inwinningszone is null)
            {
                throw new InvalidOperationException($"No Inwinningszone found for {request.Request.DownloadId}");
            }

            if (inwinningszone.Completed)
            {
                throw new InwinningszoneCompletedException(request.Request.DownloadId);
            }

            var ticketId = new TicketId(request.TicketId);
            var zipArchiveMetadata = ZipArchiveMetadata.Empty.WithInwinning();
            var translatedChanges = await _extractUploader.ProcessUploadAndDetectChanges(
                request.Request.DownloadId,
                request.Request.UploadId,
                ticketId,
                zipArchiveMetadata,
                sendFailedEmail: false,
                beforeFeatureCompare: async archive =>
                {
                    var (transactionZones, problems) = _transactionZoneFeatureCompareFeatureReader.Read(archive, FeatureType.Change, new ZipArchiveFeatureReaderContext(zipArchiveMetadata));
                    problems.ThrowIfError();

                    var extractDownload = await _extractsDbContext.ExtractDownloads.SingleAsync(x => x.DownloadId == request.Request.DownloadId.ToGuid(), cancellationToken);
                    var transactionZone = transactionZones.Single();
                    if (!transactionZone.Attributes.Geometry.Value.EqualsTopologically(extractDownload.Contour))
                    {
                        var error = ExtractFileName.Transactiezones.AtShapeRecord(FeatureType.Change, transactionZone.RecordNumber)
                            .Error(ProblemCode.TransactionZone.HasChanged)
                            .Build();
                        throw new ZipArchiveValidationException(ZipArchiveProblems.Single(error));
                    }
                },
                afterFeatureCompare: (archive, changes) =>
                {
                    var extractRoadNodes = _roadNodeFeatureCompareFeatureReader.Read(archive, FeatureType.Extract, new ZipArchiveFeatureReaderContext(zipArchiveMetadata)).Item1;
                    var actualSchijnknoopIds = extractRoadNodes
                        .Where(x => x.Attributes.Type == RoadNodeTypeV2.Schijnknoop && x.Attributes.RoadNodeId < RoadNodeConstants.InitialTemporarySchijnknoopId)
                        .Select(x => x.Attributes.RoadNodeId)
                        .ToArray();

                    if (actualSchijnknoopIds.Length == 0)
                    {
                        return Task.CompletedTask;
                    }

                    var removedSchijnknoopIds = changes.OfType<RemoveRoadNodeChange>()
                        .Where(x => actualSchijnknoopIds.Contains(x.RoadNodeId))
                        .Select(x => x.RoadNodeId)
                        .ToArray();
                    var modifiedSchijnknoopIds = changes.OfType<ModifyRoadNodeChange>()
                        .Where(x => actualSchijnknoopIds.Contains(x.RoadNodeId))
                        .Select(x => x.RoadNodeId)
                        .ToArray();
                    var unrecognizedActualSchijnknoopIds = actualSchijnknoopIds
                        .Except(removedSchijnknoopIds)
                        .Except(modifiedSchijnknoopIds)
                        .ToArray();

                    if (unrecognizedActualSchijnknoopIds.Any())
                    {
                        throw new InvalidOperationException($"BUG: upload inwinning extract (niscode {inwinningszone.NisCode}) schijnknopen {string.Join(",", unrecognizedActualSchijnknoopIds)} are not removed/modified from the extract");
                    }

                    return Task.CompletedTask;
                },
                cancellationToken);

            var migrateDryRunRoadNetworkSqsRequest = new MigrateDryRunRoadNetworkSqsRequest
            {
                TicketId = ticketId,
                ProvenanceData = new ProvenanceData(request.Provenance),
                MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
                {
                    TicketId = ticketId,
                    DownloadId = request.Request.DownloadId,
                    UploadId = request.Request.UploadId,
                    Changes = translatedChanges.Select(ChangeRoadNetworkItem.Create).ToList(),
                    ProvenanceData = new ProvenanceData(request.Provenance)
                }
            };
            await _mediator.Send(migrateDryRunRoadNetworkSqsRequest, cancellationToken);

            return new object();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while processing upload inwinning extract with download id {request.Request.DownloadId}");
            throw;
        }
    }

    protected override Task ValidateIfMatchHeaderValue(UploadInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, UploadInwinningExtractSqsLambdaRequest request)
    {
        return ConvertToDomainError(exception)?.ToTicketError(WellKnownProblemTranslators.Default)
               ?? base.InnerMapDomainException(exception, request);
    }

    private Error? ConvertToDomainError(DomainException exception)
    {
        return exception switch
        {
            UnsupportedMediaTypeException ex => ex.ContentType is not null ? new UnsupportedMediaType(ex.ContentType.Value) : new UnsupportedMediaType(),
            CorruptArchiveException => new Error(ProblemCode.Extract.CorruptArchive),
            ExtractDownloadNotFoundException ex => new ExtractNotFound(ex.DownloadId),
            ExtractRequestMarkedInformativeException => new Error(ProblemCode.Extract.CanNotUploadForInformativeExtract),
            ExtractRequestClosedException => new Error(ProblemCode.Extract.CanNotUploadForClosedExtract),
            InwinningszoneCompletedException => new Error(ProblemCode.Extract.InwinningszoneCompleted),
            CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException => new Error(ProblemCode.Extract.ExtractHasNotBeenDownloaded),
            CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException => new Error(ProblemCode.Extract.CanNotUploadForSupersededDownload),
            _ => null
        };
    }
}
