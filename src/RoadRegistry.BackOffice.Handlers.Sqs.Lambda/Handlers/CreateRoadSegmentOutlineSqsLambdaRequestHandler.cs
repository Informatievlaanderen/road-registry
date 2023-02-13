namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Framework;
using Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Requests;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;
using AddRoadSegment = BackOffice.Uploads.AddRoadSegment;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CreateRoadSegmentOutlineSqsLambdaRequestHandler : SqsLambdaHandler<CreateRoadSegmentOutlineSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

    public CreateRoadSegmentOutlineSqsLambdaRequestHandler(
        IConfiguration configuration,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<CreateRoadSegmentOutlineSqsLambdaRequestHandler> logger)
        : base(
            configuration,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _commandQueue = commandQueue;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override Task<ETagResponse> InnerHandleAsync(CreateRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
            var roadSegmentId = network.ProvidesNextRoadSegmentId()();

            var translatedChanges = TranslatedChanges.Empty
                .WithOrganization(new OrganizationId(request.Provenance.Organisation.ToString()))
                .WithOperatorName(new OperatorName(request.Provenance.Operator))
                .WithReason(new Reason("Wegsegment schetsen"));

            var r = request.Request;
            var recordNumber = RecordNumber.Initial;

            var geometry = r.Geometry;

            var fromPosition = new RoadSegmentPosition(0);
            var toPosition = new RoadSegmentPosition((decimal)geometry.Length);

            translatedChanges = translatedChanges.AppendChange(
                new AddRoadSegment(
                    recordNumber,
                    roadSegmentId,
                    r.MaintenanceAuthority,
                    RoadSegmentGeometryDrawMethod.Outlined,
                    r.Morphology,
                    r.Status,
                    RoadSegmentCategory.Unknown,
                    r.AccessRestriction)
                    .WithGeometry(geometry)
                    .WithSurface(new RoadSegmentSurfaceAttribute(network.ProvidesNextRoadSegmentSurfaceAttributeId()(roadSegmentId)(), r.SurfaceType, fromPosition, toPosition))
                    .WithWidth(new RoadSegmentWidthAttribute(network.ProvidesNextRoadSegmentWidthAttributeId()(roadSegmentId)(), r.Width, fromPosition, toPosition))
                    .WithLane(new RoadSegmentLaneAttribute(network.ProvidesNextRoadSegmentLaneAttributeId()(roadSegmentId)(), r.LaneCount, r.LaneDirection, fromPosition, toPosition))
                );

            var requestedChanges = translatedChanges.Select(change =>
            {
                var requestedChange = new RequestedChange();
                change.TranslateTo(requestedChange);
                return requestedChange;
            }).ToList();

            var messageId = Guid.NewGuid();

            var command = new ChangeRoadNetwork(request.Provenance)
            {
                RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
                Changes = requestedChanges.ToArray(),
                Reason = translatedChanges.Reason,
                Operator = translatedChanges.Operator,
                OrganizationId = translatedChanges.Organization
            };
            var commandId = command.CreateCommandId();

            await _commandQueue.Write(new Command(command).WithMessageId(commandId), cancellationToken);

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    commandId,
                    command,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetRoadSegmentHash(roadSegmentId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
        }, cancellationToken);
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, CreateRoadSegmentOutlineSqsLambdaRequest request)
    {
        return null;
    }
}
