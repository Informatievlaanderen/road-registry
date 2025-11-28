namespace RoadRegistry.SyncHost.StreetName;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Editor.Schema;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using SqlStreamStore;
using SqlStreamStore.Streams;
using Sync.StreetNameRegistry;
using StreetNameWasRemovedV2 = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRemovedV2;
using StreetNameWasRenamed = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRenamed;

public class StreetNameEventConsumer : RoadRegistryBackgroundService
{
    private readonly IStreamStore _store;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;
    private readonly IStreetNameEventTopicConsumer _consumer;
    private readonly Func<EditorContext> _editorContextFactory;

    public StreetNameEventConsumer(
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IStreetNameEventTopicConsumer consumer,
        Func<EditorContext> editorContextFactory,
        ILogger<StreetNameEventConsumer> logger
    ) : base(logger)
    {
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _roadNetworkEventWriter = roadNetworkEventWriter.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
        {
            await ConsumeHandler(message, dbContext);
        }, cancellationToken);
    }

    private async Task ConsumeHandler(object message, StreetNameEventConsumerContext dbContext)
    {
        Logger.LogInformation("Processing {Type}", message.GetType().Name);

        await (message switch
        {
            StreetNameWasRemovedV2 @event => Handle(@event),
            StreetNameWasRenamed @event => Handle(@event),
            StreetNameWasRetiredBecauseOfMunicipalityMerger @event => Handle(@event),
            StreetNameWasRejectedBecauseOfMunicipalityMerger @event => Handle(@event),
            _ => Task.CompletedTask
        });

        await dbContext.SaveChangesAsync(CancellationToken.None);

        Logger.LogInformation("Processed {Type}", message.GetType().Name);
    }

    private async Task Handle(StreetNameWasRemovedV2 message)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, CancellationToken.None);

        await LinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            StreetNameLocalId.NotApplicable,
            $"Wegsegmenten ontkoppelen van straatnaam {message.PersistentLocalId}",
            editorContext);
    }

    private async Task Handle(StreetNameWasRenamed message)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, CancellationToken.None);

        await LinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            message.DestinationPersistentLocalId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {message.DestinationPersistentLocalId}",
            editorContext);

        await WriteStreetNameRenamedEvent(message.PersistentLocalId, message.DestinationPersistentLocalId);
    }

    private async Task Handle(StreetNameWasRetiredBecauseOfMunicipalityMerger message)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, CancellationToken.None, waitDelayMilliseconds: 250);

        var destinationStreetNameId = message.NewPersistentLocalIds.Any()
            ? message.NewPersistentLocalIds.First()
            : StreetNameLocalId.NotApplicable;

        await LinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            destinationStreetNameId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {destinationStreetNameId} in functie van een gemeentefusie",
            editorContext);

        await WriteStreetNameRenamedEvent(message.PersistentLocalId, destinationStreetNameId);
    }

    private async Task Handle(StreetNameWasRejectedBecauseOfMunicipalityMerger message)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, CancellationToken.None, waitDelayMilliseconds: 250);

        var destinationStreetNameId = message.NewPersistentLocalIds.Any()
            ? message.NewPersistentLocalIds.First()
            : StreetNameLocalId.NotApplicable;

        await LinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            destinationStreetNameId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {destinationStreetNameId} in functie van een gemeentefusie",
            editorContext);

        await WriteStreetNameRenamedEvent(message.PersistentLocalId, destinationStreetNameId);
    }

    private async Task LinkRoadSegmentsToDifferentStreetName(int sourceStreetNameId, int destinationStreetNameId, string reason, EditorContext editorContext)
    {
        var cancellationToken = CancellationToken.None;

        var segments = await editorContext.RoadSegments
            .IncludeLocalToListAsync(q => q.Where(x => x.LeftSideStreetNameId == sourceStreetNameId || x.RightSideStreetNameId == sourceStreetNameId), cancellationToken);
        if (!segments.Any())
        {
            return;
        }

        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost, Logger, cancellationToken, waitDelayMilliseconds: 250);

        var segmentsPerStreamGrouping = segments
            .GroupBy(x => x.MethodId == RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier
                ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(x.Id))
                : RoadNetworkStreamNameProvider.Default);

        foreach (var segmentsPerStream in segmentsPerStreamGrouping)
        {
            var roadSegmentChanges = new List<RoadSegmentStreetNamesChanged>();

            foreach (var roadSegment in segmentsPerStream)
            {
                var modifyRoadSegment = new RoadSegmentStreetNamesChanged
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegment.MethodId],
                    Id = roadSegment.Id,
                    Version = roadSegment.Version + 1
                };
                if (roadSegment.LeftSideStreetNameId == sourceStreetNameId)
                {
                    modifyRoadSegment.LeftSideStreetNameId = destinationStreetNameId;
                }
                if (roadSegment.RightSideStreetNameId == sourceStreetNameId)
                {
                    modifyRoadSegment.RightSideStreetNameId = destinationStreetNameId;
                }

                roadSegmentChanges.Add(modifyRoadSegment);

                Logger.LogInformation("Linking RoadSegment {Id} to StreetName {StreetNameId}s", roadSegment.Id, destinationStreetNameId);
            }

            var @event = new RoadSegmentsStreetNamesChanged
            {
                Reason = reason,
                RoadSegments = roadSegmentChanges.ToArray()
            };

            await _roadNetworkEventWriter.WriteAsync(segmentsPerStream.Key, ExpectedVersion.Any, new Event(@event), cancellationToken);
        }
    }

    private async Task WriteStreetNameRenamedEvent(int streetNameLocalId, int destinationStreetNameLocalId)
    {
        var streetNameEvent = new Event(new StreetNameRenamed
        {
            StreetNameLocalId = streetNameLocalId,
            DestinationStreetNameLocalId = destinationStreetNameLocalId
        });
        await _streetNameEventWriter.WriteAsync(streetNameEvent, CancellationToken.None);
    }
}
