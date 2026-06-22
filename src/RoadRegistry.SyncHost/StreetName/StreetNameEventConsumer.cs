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
using BackOffice.Handlers.Sqs;
using BackOffice.Handlers.Sqs.SystemFlows;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Editor.Schema;
using Hosts;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Read.Projections;
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
    private readonly IDocumentStore _documentStore;
    private readonly IBackOfficeS3SqsQueue _sqsQueue;

    public StreetNameEventConsumer(
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IStreetNameEventTopicConsumer consumer,
        Func<EditorContext> editorContextFactory,
        IDocumentStore documentStore,
        IBackOfficeS3SqsQueue sqsQueue,
        ILogger<StreetNameEventConsumer> logger
    ) : base(logger)
    {
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _roadNetworkEventWriter = roadNetworkEventWriter.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
        _documentStore = documentStore.ThrowIfNull();
        _sqsQueue = sqsQueue.ThrowIfNull();
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

        // V1 road segments are tracked in the (SQL) editor read model and relinked through the road network event stream.
        var v1Segments = await editorContext.RoadSegments
            .IncludeLocalToListAsync(q => q.Where(x => x.LeftSideStreetNameId == sourceStreetNameId || x.RightSideStreetNameId == sourceStreetNameId), cancellationToken);
        if (v1Segments.Any())
        {
            await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost, Logger, cancellationToken, waitDelayMilliseconds: 250);

            var segmentsPerStreamGrouping = v1Segments
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

                    Logger.LogInformation("Linking RoadSegment {Id} to StreetName {StreetNameId}", roadSegment.Id, destinationStreetNameId);
                }

                var @event = new RoadSegmentsStreetNamesChanged
                {
                    Reason = reason,
                    RoadSegments = roadSegmentChanges.ToArray()
                };

                await _roadNetworkEventWriter.WriteAsync(segmentsPerStream.Key, ExpectedVersion.Any, new Event(@event), cancellationToken);
            }
        }

        // V2 road segments are tracked in the Marten read model and relinked directly through a system SQS request.
        await _documentStore.WaitForNonStaleProjection(WellKnownProjectionStateNames.RoadNetworkChangesReadProjection, Logger, cancellationToken);

        var v2RoadSegmentIds = new List<RoadSegmentId>();
        await using (var session = _documentStore.LightweightSession())
        {
            var link = await session.LoadAsync<StreetNameRoadSegmentsLink>(sourceStreetNameId, cancellationToken);
            if (link is not null && link.RoadSegmentIds.Any())
            {
                var readItems = await session.LoadManyAsync<RoadSegmentReadItem>(cancellationToken, link.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
                v2RoadSegmentIds.AddRange(readItems.Where(x => x is { IsV2: true, IsRemoved: false }).Select(x => x.RoadSegmentId));
            }
        }

        if (v2RoadSegmentIds.Any())
        {
            var request = new SystemLinkRoadSegmentsToStreetNameIdsSqsRequest
            {
                TicketId = Guid.NewGuid(),
                ProvenanceData = new RoadRegistryProvenanceData(Modification.Update),
                RoadSegmentIds = v2RoadSegmentIds,
                OldStreetNameId = new StreetNameLocalId(sourceStreetNameId),
                NewStreetNameId = new StreetNameLocalId(destinationStreetNameId)
            };

            await _sqsQueue.Copy(
                request,
                new SqsQueueOptions { MessageGroupId = RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork.Constants.GlobalMessageGroupId },
                cancellationToken);

            Logger.LogInformation("Published system request to relink {Count} V2 RoadSegments from StreetName {Source} to {Destination}", v2RoadSegmentIds.Count, sourceStreetNameId, destinationStreetNameId);
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
