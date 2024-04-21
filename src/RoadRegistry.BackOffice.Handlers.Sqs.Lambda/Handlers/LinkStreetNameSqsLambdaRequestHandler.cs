namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Extensions;
using Hosts;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Requests;
using StreetName;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;
using StreetNameStatus = Syndication.Schema.StreetNameStatus;

public sealed class LinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<LinkStreetNameSqsLambdaRequest>
{
    private readonly IStreetNameClient _streetNameClient;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly DistributedStreamStoreLockOptions _distributedStreamStoreLockOptions;

    private static readonly string[] ProposedOrCurrentStreetNameStatuses = new[]
    {
        StreetNameStatus.Current.ToString(),
        StreetNameStatus.Proposed.ToString(),
        RoadRegistry.StreetName.StreetNameStatus.Current,
        RoadRegistry.StreetName.StreetNameStatus.Proposed
    };

    public LinkStreetNameSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IStreetNameClient streetNameClient,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<LinkStreetNameSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _streetNameClient = streetNameClient;
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _distributedStreamStoreLockOptions = distributedStreamStoreLockOptions;
    }

    protected override async Task<object> InnerHandle(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var roadSegmentId = new RoadSegmentId(request.Request.WegsegmentId);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(request.Request.Methode);
        var streamName = RoadNetworkStreamNameProvider.Get(roadSegmentId, geometryDrawMethod);
        var distributedStreamStoreLock = new DistributedStreamStoreLock(_distributedStreamStoreLockOptions, streamName, Logger);

        await distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Straatnaam koppelen", async translatedChanges =>
            {
                var problems = Problems.None;

                var roadSegment = await RoadRegistryContext.RoadNetworks.FindRoadSegment(roadSegmentId, geometryDrawMethod, cancellationToken);
                if (roadSegment == null)
                {
                    problems += new RoadSegmentNotFound(roadSegmentId);
                    throw new RoadRegistryProblemsException(problems);
                }

                var recordNumber = RecordNumber.Initial;
                var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

                var leftStreetNameId = request.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightStreetNameId = request.Request.RechterstraatnaamId.GetIdentifierFromPuri();

                if (leftStreetNameId > 0 || rightStreetNameId > 0)
                {
                    if (leftStreetNameId > 0)
                    {
                        if (!StreetNameLocalId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
                        {
                            problems += new RoadSegmentStreetNameLeftNotUnlinked(request.Request.WegsegmentId);
                        }
                        else
                        {
                            problems = await ValidateStreetName(leftStreetNameId, problems, cancellationToken);
                        }
                    }

                    if (rightStreetNameId > 0)
                    {
                        if (!StreetNameLocalId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId))
                        {
                            problems = problems.Add(new RoadSegmentStreetNameRightNotUnlinked(request.Request.WegsegmentId));
                        }
                        else
                        {
                            problems = await ValidateStreetName(rightStreetNameId, problems, cancellationToken);
                        }
                    }

                    var modifyRoadSegment = new ModifyRoadSegment(
                        recordNumber,
                        roadSegment.Id,
                        roadSegment.Start,
                        roadSegment.End,
                        roadSegment.AttributeHash.OrganizationId,
                        roadSegment.AttributeHash.GeometryDrawMethod,
                        roadSegment.AttributeHash.Morphology,
                        roadSegment.AttributeHash.Status,
                        roadSegment.AttributeHash.Category,
                        roadSegment.AttributeHash.AccessRestriction,
                        leftStreetNameId > 0 ? new StreetNameLocalId(leftStreetNameId) : roadSegment.AttributeHash.LeftStreetNameId,
                        rightStreetNameId > 0 ? new StreetNameLocalId(rightStreetNameId) : roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry);

                    foreach (var lane in roadSegment.Lanes)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, lane.From, lane.To));
                    }
                    foreach (var surface in roadSegment.Surfaces)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, surface.From, surface.To));
                    }
                    foreach (var width in roadSegment.Widths)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, width.From, width.To));
                    }

                    translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);
                }

                if (problems.Any())
                {
                    throw new RoadRegistryProblemsException(problems);
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        var lastHash = await GetRoadSegmentHash(roadSegmentId, geometryDrawMethod, cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task<Problems> ValidateStreetName(int streetNameId, Problems problems, CancellationToken cancellationToken)
    {
        try
        {
            var streetName = await _streetNameClient.GetAsync(streetNameId, cancellationToken);
            if (streetName is null)
            {
                return problems.Add(new StreetNameNotFound());
            }

            if (ProposedOrCurrentStreetNameStatuses.All(status => !string.Equals(streetName.Status, status, StringComparison.InvariantCultureIgnoreCase)))
            {
                return problems.Add(new RoadSegmentStreetNameNotProposedOrCurrent());
            }
        }
        catch (StreetNameRegistryUnexpectedStatusCodeException ex)
        {
            Logger.LogError(ex.Message);

            problems += new StreetNameRegistryUnexpectedError((int)ex.StatusCode);
        }

        return problems;
    }
}
