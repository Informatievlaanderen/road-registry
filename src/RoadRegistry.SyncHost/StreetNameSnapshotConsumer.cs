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
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Editor.Schema.Extensions;
using FluentValidation;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using SqlStreamStore;
using StreetName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StreetNameRecord = BackOffice.Messages.StreetNameRecord;

public class StreetNameSnapshotConsumer : RoadRegistryBackgroundService
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _encoding;
    private readonly UseRoadSegmentV2EventProcessorFeatureToggle _useRoadSegmentV2EventProcessorFeatureToggle;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly IStreetNameSnapshotTopicConsumer _consumer;
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameSnapshotConsumer(
        ILifetimeScope container,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IStreetNameSnapshotTopicConsumer consumer,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding encoding,
        UseRoadSegmentV2EventProcessorFeatureToggle useRoadSegmentV2EventProcessorFeatureToggle,
        ILogger<StreetNameSnapshotConsumer> logger
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
        _useRoadSegmentV2EventProcessorFeatureToggle = useRoadSegmentV2EventProcessorFeatureToggle.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
            {
                Logger.LogInformation("Processing streetname {Key}", message.Key);

                var map = _container.Resolve<EventSourcedEntityMap>();
                var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

                var snapshotRecord = (StreetNameSnapshotRecord)message.Value;
                var streetNameId = StreetNamePuri.FromValue(message.Key);
                var streetNameLocalId = streetNameId.ToStreetNameLocalId();

                var streetNameEventSourced = await streetNamesContext.FindAsync(streetNameLocalId, cancellationToken);

                var streetNameDbRecord = new StreetNameRecord
                {
                    StreetNameId = streetNameId,
                    PersistentLocalId = streetNameLocalId,
                    NisCode = snapshotRecord?.Gemeente?.ObjectId,

                    DutchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.NL),
                    FrenchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.FR),
                    GermanName = GetSpelling(snapshotRecord?.Straatnamen, Taal.DE),
                    EnglishName = GetSpelling(snapshotRecord?.Straatnamen, Taal.EN),

                    DutchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.NL),
                    FrenchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.FR),
                    GermanHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.DE),
                    EnglishHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.EN),

                    StreetNameStatus = snapshotRecord?.StraatnaamStatus
                };

                var @event = DetermineEvent(streetNameEventSourced, streetNameDbRecord);
                
                Command roadNetworkCommand = null;

                if (@event is StreetNameRemoved)
                {
                    var waitForProjectionStateName = _useRoadSegmentV2EventProcessorFeatureToggle.FeatureEnabled
                        ? WellKnownProjectionStateNames.RoadRegistryEditorRoadSegmentV2ProjectionHost
                        : WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

                    using (var editorContext = _editorContextFactory())
                    {
                        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, waitForProjectionStateName, Logger, cancellationToken);

                        var changeRoadNetwork = await BuildChangeRoadNetworkToUnlinkRoadSegmentsFromStreetName(streetNameLocalId, editorContext, cancellationToken);
                        if (changeRoadNetwork is not null)
                        {
                            roadNetworkCommand = new Command(changeRoadNetwork);
                        }
                    }
                }

                await _streetNameEventWriter.WriteAsync(streetNameLocalId, new Event(@event), cancellationToken);

                if (roadNetworkCommand is not null)
                {
                    await _roadNetworkCommandQueue.WriteAsync(roadNetworkCommand, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Processed streetname {Key}", message.Key);
            }, cancellationToken);
    }

    private async Task<ChangeRoadNetwork> BuildChangeRoadNetworkToUnlinkRoadSegmentsFromStreetName(int streetNameId, EditorContext editorContext, CancellationToken cancellationToken)
    {
        var segments = await editorContext.RoadSegmentsV2
            .Where(x => x.LeftSideStreetNameId == streetNameId || x.RightSideStreetNameId == streetNameId)
            .ToListAsync(cancellationToken);

        if (!segments.Any())
        {
            return null;
        }

        var roadSegmentIds = segments.Select(x => x.Id).ToList();

        var lanes = await editorContext.RoadSegmentLaneAttributes
            .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
            .ToListAsync(cancellationToken);
        var surfaces = await editorContext.RoadSegmentLaneAttributes
            .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
            .ToListAsync(cancellationToken);
        var widths = await editorContext.RoadSegmentLaneAttributes
            .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
            .ToListAsync(cancellationToken);

        var reason = $"Wegsegmenten ontkoppelen van straatnaam {streetNameId}";
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
                roadSegment.LeftSideStreetNameId == streetNameId ? CrabStreetNameId.NotApplicable : CrabStreetNameId.FromValue(roadSegment.LeftSideStreetNameId),
                roadSegment.RightSideStreetNameId == streetNameId ? CrabStreetNameId.NotApplicable : CrabStreetNameId.FromValue(roadSegment.RightSideStreetNameId)
            ).WithGeometry(roadSegment.Geometry.ToMultiLineString());

            var roadSegmentLanes = lanes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentLaneAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();
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
            
            var roadSegmentSurfaces = surfaces
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentSurfaceAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();
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

            var roadSegmentWidths = widths
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .Select(x => new RoadSegmentWidthAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _encoding))
                .Where(x => x.VANPOS.Value.HasValue && x.TOTPOS.Value.HasValue)
                .ToList();
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

    private static object DetermineEvent(StreetName streetNameEventSourced, StreetNameRecord streetNameDbRecord)
    {
        if (streetNameEventSourced is null)
        {
            return new StreetNameCreated
            {
                Record = streetNameDbRecord
            };
        }

        if (streetNameDbRecord.StreetNameStatus is null)
        {
            return new StreetNameRemoved
            {
                StreetNameId = streetNameEventSourced.StreetNameId
            };
        }

        return new StreetNameModified
        {
            Record = streetNameDbRecord,
            NameModified = streetNameEventSourced.DutchName != streetNameDbRecord.DutchName
                           || streetNameEventSourced.EnglishName != streetNameDbRecord.EnglishName
                           || streetNameEventSourced.FrenchName != streetNameDbRecord.FrenchName
                           || streetNameEventSourced.GermanName != streetNameDbRecord.GermanName,
            HomonymAdditionModified = streetNameEventSourced.DutchHomonymAddition != streetNameDbRecord.DutchHomonymAddition
                                     || streetNameEventSourced.EnglishHomonymAddition != streetNameDbRecord.EnglishHomonymAddition
                                     || streetNameEventSourced.FrenchHomonymAddition != streetNameDbRecord.FrenchHomonymAddition
                                     || streetNameEventSourced.GermanHomonymAddition != streetNameDbRecord.GermanHomonymAddition,
            StatusModified = streetNameEventSourced.StreetNameStatus != streetNameDbRecord.StreetNameStatus,
            Restored = streetNameEventSourced.IsRemoved
        };
    }

    private static string GetSpelling(List<DeseriazableGeografischeNaam> namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
