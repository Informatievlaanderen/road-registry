namespace RoadRegistry.SyncHost;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.FeatureToggles;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Editor.Schema.Extensions;
using FluentValidation;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using SqlStreamStore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StreetNameWasRemovedV2 = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRemovedV2;
using StreetNameWasRenamed = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRenamed;

public class StreetNameEventConsumer : RoadRegistryBackgroundService
{
    private readonly ILifetimeScope _container;
    private readonly IStreamStore _store;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IStreetNameEventTopicConsumer _consumer;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly FileEncoding _encoding;

    public StreetNameEventConsumer(
        ILifetimeScope container,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IStreetNameEventTopicConsumer consumer,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding encoding,
        ILogger<StreetNameEventConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _roadNetworkCommandQueue = roadNetworkCommandQueue.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
        _manager = manager.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
            {
                Logger.LogInformation("Processing {Type}", message.GetType().Name);
                
                Command roadNetworkCommand = null;

                if (message is StreetNameWasRemovedV2 streetNameWasRemoved)
                {
                    roadNetworkCommand = await BuildRoadNetworkCommand(streetNameWasRemoved, cancellationToken);
                }
                else if (message is StreetNameWasRenamed streetNameWasRenamed)
                {
                    roadNetworkCommand = await BuildRoadNetworkCommand(streetNameWasRenamed, cancellationToken);

                    var streetNameEvent = BuildStreetNameEvent(streetNameWasRenamed);
                    await _streetNameEventWriter.WriteAsync(streetNameEvent, cancellationToken);
                }

                if (roadNetworkCommand is not null)
                {
                    await _roadNetworkCommandQueue.WriteAsync(roadNetworkCommand, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Processed {Type}", message.GetType().Name);
            }, cancellationToken);
    }

    private Event BuildStreetNameEvent(StreetNameWasRenamed message)
    {
        return new Event(new StreetNameRenamed
        {
            StreetNameLocalId = message.PersistentLocalId,
            DestinationStreetNameLocalId = message.DestinationPersistentLocalId
        });
    }
    
    private async Task<Command> BuildRoadNetworkCommand(StreetNameWasRemovedV2 message, CancellationToken cancellationToken)
    {
        var waitForProjectionStateName = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

        using (var editorContext = _editorContextFactory())
        {
            await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, waitForProjectionStateName, Logger, cancellationToken);

            var changeRoadNetwork = await BuildChangeRoadNetworkToConnectRoadSegmentsToDifferentStreetName(
                message.PersistentLocalId,
                StreetNameLocalId.NotApplicable,
                $"Wegsegmenten ontkoppelen van straatnaam {message.PersistentLocalId}",
                editorContext,
                cancellationToken);
            if (changeRoadNetwork is not null)
            {
                return new Command(changeRoadNetwork);
            }
        }

        return null;
    }

    private async Task<Command> BuildRoadNetworkCommand(StreetNameWasRenamed message, CancellationToken cancellationToken)
    {
        var waitForProjectionStateName = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

        using (var editorContext = _editorContextFactory())
        {
            await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, waitForProjectionStateName, Logger, cancellationToken);

            var changeRoadNetwork = await BuildChangeRoadNetworkToConnectRoadSegmentsToDifferentStreetName(
                message.PersistentLocalId,
                message.DestinationPersistentLocalId,
                $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {message.DestinationPersistentLocalId}",
                editorContext,
                cancellationToken);
            if (changeRoadNetwork is not null)
            {
                return new Command(changeRoadNetwork);
            }
        }

        return null;
    }

    private async Task<ChangeRoadNetwork> BuildChangeRoadNetworkToConnectRoadSegmentsToDifferentStreetName(int sourceStreetNameId, int destinationStreetNameId, string reason, EditorContext editorContext, CancellationToken cancellationToken)
    {
        var segments = await editorContext.RoadSegments
            .ToListIncludingLocalAsync(x => x.LeftSideStreetNameId == sourceStreetNameId || x.RightSideStreetNameId == sourceStreetNameId, cancellationToken);
        if (!segments.Any())
        {
            return null;
        }

        var roadSegmentIds = segments.Select(x => x.Id).ToList();

        var lanes = await editorContext.RoadSegmentLaneAttributes
            .ToListIncludingLocalAsync(x => roadSegmentIds.Contains(x.RoadSegmentId), cancellationToken);
        var surfaces = await editorContext.RoadSegmentSurfaceAttributes
            .ToListIncludingLocalAsync(x => roadSegmentIds.Contains(x.RoadSegmentId), cancellationToken);
        var widths = await editorContext.RoadSegmentWidthAttributes
            .ToListIncludingLocalAsync(x => roadSegmentIds.Contains(x.RoadSegmentId), cancellationToken);

        var organizationId = OrganizationId.DigitaalVlaanderen;

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(organizationId)
            .WithOperatorName(new OperatorName(organizationId))
            .WithReason(new Reason(reason));

        var recordNumber = RecordNumber.Initial;
        var attributeId = AttributeId.Initial;

        foreach (var roadSegment in segments)
        {
            var modifyRoadSegment = new BackOffice.Uploads.ModifyRoadSegment(
                recordNumber,
                new RoadSegmentId(roadSegment.Id),
                new RoadNodeId(roadSegment.StartNodeId),
                new RoadNodeId(roadSegment.EndNodeId),
                new OrganizationId(roadSegment.MaintainerId),
                RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegment.MethodId],
                RoadSegmentMorphology.ByIdentifier[roadSegment.MorphologyId],
                RoadSegmentStatus.ByIdentifier[roadSegment.StatusId],
                RoadSegmentCategory.ByIdentifier[roadSegment.CategoryId],
                RoadSegmentAccessRestriction.ByIdentifier[roadSegment.AccessRestrictionId],
                roadSegment.LeftSideStreetNameId == sourceStreetNameId ? StreetNameLocalId.FromValue(destinationStreetNameId) : StreetNameLocalId.FromValue(roadSegment.LeftSideStreetNameId),
                roadSegment.RightSideStreetNameId == sourceStreetNameId ? StreetNameLocalId.FromValue(destinationStreetNameId) : StreetNameLocalId.FromValue(roadSegment.RightSideStreetNameId)
            ).WithGeometry(roadSegment.Geometry.ToMultiLineString());
            
            var roadSegmentLanes = lanes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentLaneAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();
            var roadSegmentSurfaces = surfaces
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentSurfaceAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();
            var roadSegmentWidths = widths
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentWidthAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();

            Logger.LogInformation("Linking RoadSegment {Id} to StreetName {StreetNameId}, {Lanes} lanes, {Surfaces} surfaces, {Widths} widths", roadSegment.Id, destinationStreetNameId, roadSegmentLanes.Count, roadSegmentSurfaces.Count, roadSegmentWidths.Count);

            if (roadSegmentLanes.Any())
            {
                foreach (var lane in roadSegmentLanes)
                {
                    modifyRoadSegment = modifyRoadSegment.WithLane(new BackOffice.Uploads.RoadSegmentLaneAttribute(
                        attributeId,
                        new RoadSegmentLaneCount(lane.AANTAL.Value),
                        RoadSegmentLaneDirection.ByIdentifier[lane.RICHTING.Value],
                        RoadSegmentPosition.FromDouble(lane.VANPOS.Value!.Value),
                        RoadSegmentPosition.FromDouble(lane.TOTPOS.Value!.Value)
                    ));
                    attributeId = attributeId.Next();
                }
            }
            else
            {
                modifyRoadSegment = modifyRoadSegment.WithLane(new BackOffice.Uploads.RoadSegmentLaneAttribute(
                    attributeId,
                    RoadSegmentLaneCount.Unknown,
                    RoadSegmentLaneDirection.Unknown,
                    new RoadSegmentPosition(0),
                    RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)
                ));
                attributeId = attributeId.Next();
            }
            
            if (roadSegmentSurfaces.Any())
            {
                foreach (var surface in roadSegmentSurfaces)
                {
                    modifyRoadSegment = modifyRoadSegment.WithSurface(new BackOffice.Uploads.RoadSegmentSurfaceAttribute(
                        attributeId,
                        RoadSegmentSurfaceType.ByIdentifier[surface.TYPE.Value],
                        RoadSegmentPosition.FromDouble(surface.VANPOS.Value!.Value),
                        RoadSegmentPosition.FromDouble(surface.TOTPOS.Value!.Value)
                    ));
                    attributeId = attributeId.Next();
                }
            }
            else
            {
                modifyRoadSegment = modifyRoadSegment.WithSurface(new BackOffice.Uploads.RoadSegmentSurfaceAttribute(
                    attributeId,
                    RoadSegmentSurfaceType.Unknown,
                    new RoadSegmentPosition(0),
                    RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)
                ));
                attributeId = attributeId.Next();
            }
            
            if (roadSegmentWidths.Any())
            {
                foreach (var width in roadSegmentWidths)
                {
                    modifyRoadSegment = modifyRoadSegment.WithWidth(new BackOffice.Uploads.RoadSegmentWidthAttribute(
                        attributeId,
                        new RoadSegmentWidth(width.BREEDTE.Value),
                        RoadSegmentPosition.FromDouble(width.VANPOS.Value!.Value),
                        RoadSegmentPosition.FromDouble(width.TOTPOS.Value!.Value)
                    ));
                    attributeId = attributeId.Next();
                }
            }
            else
            {
                modifyRoadSegment = modifyRoadSegment.WithWidth(new BackOffice.Uploads.RoadSegmentWidthAttribute(
                    attributeId,
                    RoadSegmentWidth.Unknown,
                    new RoadSegmentPosition(0),
                    RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)
                ));
                attributeId = attributeId.Next();
            }

            translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);

        return changeRoadNetwork;
    }
}
