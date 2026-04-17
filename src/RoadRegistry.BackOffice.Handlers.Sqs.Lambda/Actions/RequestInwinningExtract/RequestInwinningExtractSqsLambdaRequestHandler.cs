namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestInwinningExtract;

using System.IO.Compression;
using System.Text;
using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using RequestExtract;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using TicketingService.Abstractions;

public sealed class RequestInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestInwinningExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly ExtractRequester _extractRequester;
    private readonly RoadSegmentFeatureCompareFeatureReader _roadSegmentFeatureCompareFeatureReader;

    public RequestInwinningExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        ExtractRequester extractRequester,
        RoadSegmentFeatureCompareFeatureReader roadSegmentFeatureCompareFeatureReader,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<RequestInwinningExtractSqsLambdaRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _extractRequester = extractRequester;
        _roadSegmentFeatureCompareFeatureReader = roadSegmentFeatureCompareFeatureReader;
    }

    protected override async Task<object> InnerHandle(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var niscode = request.Request.NisCode;
            var geometry = request.Request.Contour.Value;

            if (!request.Request.IsInformative)
            {
                var inwinningsZone = await _extractsDbContext.Inwinningszones.FindAsync([niscode], cancellationToken);

                if (inwinningsZone is not null && inwinningsZone.Operator != request.Provenance.Operator)
                {
                    throw new ValidationException([
                        new ValidationFailure
                        {
                            PropertyName = string.Empty,
                            ErrorCode = "InwinningszoneGestart",
                            ErrorMessage = "Inwinning is al gestart door een andere organisatie."
                        }
                    ]);
                }

                if (inwinningsZone is null)
                {
                    inwinningsZone = new Inwinningszone
                    {
                        NisCode = niscode,
                        Contour = geometry,
                        Operator = request.Provenance.Operator,
                        DownloadId = request.Request.DownloadId,
                        Completed = false
                    };
                    _extractsDbContext.Inwinningszones.Add(inwinningsZone);
                }
                else
                {
                    inwinningsZone.DownloadId = request.Request.DownloadId;
                }
            }

            await _extractRequester.BuildExtract(
                new RequestExtractData(
                    request.Request.ExtractRequestId,
                    request.Request.DownloadId,
                    geometry,
                    request.Request.Description,
                    request.Request.IsInformative,
                    BuildExternalRequestId(niscode)
                ),
                new TicketId(request.TicketId),
                WellKnownZipArchiveWriterVersions.DomainV2_Inwinning,
                request.Provenance,
                onArchiveBuilt: archiveStream =>
                {
                    if (archiveStream.Length > 0)
                    {
                        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, true, Encoding.UTF8);

                        var (roadSegments, _) = _roadSegmentFeatureCompareFeatureReader.Read(archive, FeatureType.Extract, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
                        _extractsDbContext.InwinningRoadSegments.AddRange(roadSegments
                            .Select(x => x.Attributes.RoadSegmentId!.Value)
                            .Distinct()
                            .Select(roadSegmentId => new InwinningRoadSegment
                            {
                                NisCode = niscode,
                                RoadSegmentId = roadSegmentId,
                                Completed = false
                            }));
                    }
                },
                cancellationToken);

            var downloadId = new DownloadId(request.Request.DownloadId);
            return new RequestExtractResponse(downloadId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error trying to build an extract for niscode {request.Request.NisCode}");
            throw;
        }
    }

    private static string BuildExternalRequestId(string niscode)
    {
        // don't change the format, it's used in backend and frontend
        return $"INWINNING_{niscode}";
    }

    protected override Task ValidateIfMatchHeaderValue(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
