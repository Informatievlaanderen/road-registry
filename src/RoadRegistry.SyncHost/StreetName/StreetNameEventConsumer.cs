namespace RoadRegistry.SyncHost.StreetName;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Hosts;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using SqlStreamStore;
using SqlStreamStore.Streams;
using AcceptedChange = BackOffice.Core.AcceptedChange;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using ModifyRoadSegmentAttributes = BackOffice.Core.ModifyRoadSegmentAttributes;
using RoadSegmentSideAttributes = BackOffice.Core.RoadSegmentSideAttributes;
using StreetNameWasRemovedV2 = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRemovedV2;
using StreetNameWasRenamed = Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRenamed;

public class StreetNameEventConsumer : RoadRegistryBackgroundService
{
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IStreamStore _store;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;
    private readonly IStreetNameEventTopicConsumer _consumer;
    private readonly Func<EditorContext> _editorContextFactory;

    public StreetNameEventConsumer(
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IStreetNameEventTopicConsumer consumer,
        Func<EditorContext> editorContextFactory,
        ILogger<StreetNameEventConsumer> logger
    ) : base(logger)
    {
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _roadNetworkEventWriter = roadNetworkEventWriter.ThrowIfNull();
        _roadNetworkIdGenerator = roadNetworkIdGenerator.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
            {
                Logger.LogInformation("Processing {Type}", message.GetType().Name);

                await (message switch
                {
                    StreetNameWasRemovedV2 @event => Handle(@event, cancellationToken),
                    StreetNameWasRenamed @event => Handle(@event, cancellationToken),
                    StreetNameWasRetiredBecauseOfMunicipalityMerger @event => Handle(@event, cancellationToken),
                    StreetNameWasRejectedBecauseOfMunicipalityMerger @event => Handle(@event, cancellationToken),
                    _ => Task.CompletedTask
                });

                await dbContext.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Processed {Type}", message.GetType().Name);
            }, cancellationToken);
    }

    private async Task Handle(StreetNameWasRemovedV2 message, CancellationToken cancellationToken)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, cancellationToken);

        var roadNetworkEvent = await BuildRoadNetworkEventToLinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            StreetNameLocalId.NotApplicable,
            $"Wegsegmenten ontkoppelen van straatnaam {message.PersistentLocalId}",
            editorContext,
            cancellationToken);
        if (roadNetworkEvent is not null)
        {
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, roadNetworkEvent, cancellationToken);
        }
    }

    private async Task Handle(StreetNameWasRenamed message, CancellationToken cancellationToken)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, cancellationToken);

        var roadNetworkEvent = await BuildRoadNetworkEventToLinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            message.DestinationPersistentLocalId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {message.DestinationPersistentLocalId}",
            editorContext,
            cancellationToken);
        if (roadNetworkEvent is not null)
        {
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, roadNetworkEvent, cancellationToken);
        }

        var streetNameEvent = BuildStreetNameEvent(message);
        await _streetNameEventWriter.WriteAsync(streetNameEvent, cancellationToken);
    }

    private async Task Handle(StreetNameWasRetiredBecauseOfMunicipalityMerger message, CancellationToken cancellationToken)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, cancellationToken, waitDelayMilliseconds: 250);

        var destinationStreetNameId = message.NewPersistentLocalIds.Any()
            ? message.NewPersistentLocalIds.First()
            : StreetNameLocalId.NotApplicable;

        var roadNetworkEvent = await BuildRoadNetworkEventToLinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            destinationStreetNameId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {destinationStreetNameId} in functie van een gemeentefusie",
            editorContext,
            cancellationToken);
        if (roadNetworkEvent is not null)
        {
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, roadNetworkEvent, cancellationToken);
        }
    }

    private async Task Handle(StreetNameWasRejectedBecauseOfMunicipalityMerger message, CancellationToken cancellationToken)
    {
        await using var editorContext = _editorContextFactory();
        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, Logger, cancellationToken, waitDelayMilliseconds: 250);

        var destinationStreetNameId = message.NewPersistentLocalIds.Any()
            ? message.NewPersistentLocalIds.First()
            : StreetNameLocalId.NotApplicable;

        var roadNetworkEvent = await BuildRoadNetworkEventToLinkRoadSegmentsToDifferentStreetName(
            message.PersistentLocalId,
            destinationStreetNameId,
            $"Wegsegmenten herkoppelen van straatnaam {message.PersistentLocalId} naar {destinationStreetNameId} in functie van een gemeentefusie",
            editorContext,
            cancellationToken);
        if (roadNetworkEvent is not null)
        {
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, roadNetworkEvent, cancellationToken);
        }
    }

    private async Task<Event?> BuildRoadNetworkEventToLinkRoadSegmentsToDifferentStreetName(int sourceStreetNameId, int destinationStreetNameId, string reason, EditorContext editorContext, CancellationToken cancellationToken)
    {
        var segments = await editorContext.RoadSegments
            .IncludeLocalToListAsync(q => q.Where(x => x.LeftSideStreetNameId == sourceStreetNameId || x.RightSideStreetNameId == sourceStreetNameId), cancellationToken);
        if (!segments.Any())
        {
            return null;
        }

        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost, Logger, cancellationToken, waitDelayMilliseconds: 250);
        var organization = await editorContext.OrganizationsV2.IncludeLocalSingleOrDefaultAsync(x => x.OvoCode == OrganizationOvoCode.DigitaalVlaanderen, cancellationToken);

        var transactionId = await _roadNetworkIdGenerator.NewTransactionId();
        var requestedChanges = RequestedChanges.Start(transactionId);

        foreach (var roadSegment in segments)
        {
            var leftSide = roadSegment.LeftSideStreetNameId == sourceStreetNameId
                ? new RoadSegmentSideAttributes(new StreetNameLocalId(destinationStreetNameId))
                : null;
            var rightSide = roadSegment.RightSideStreetNameId == sourceStreetNameId
                ? new RoadSegmentSideAttributes(new StreetNameLocalId(destinationStreetNameId))
                : null;

            requestedChanges = requestedChanges.Append(new ModifyRoadSegmentAttributes(
                new RoadSegmentId(roadSegment.Id),
                new RoadSegmentVersion(roadSegment.Version + 1),
                RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegment.MethodId],
                null, null, null, null, null, null,
                leftSide,
                rightSide,
                null, null, null
            ));

            Logger.LogInformation("Linking RoadSegment {Id} to StreetName {StreetNameId}s", roadSegment.Id, destinationStreetNameId);
        }

        var @event = new RoadNetworkChangesAccepted
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            Reason = reason,
            Operator = new OperatorName(organization.Code),
            OrganizationId = organization.Code,
            Organization = organization.Name,
            TransactionId = requestedChanges.TransactionId,
            Changes = requestedChanges
                .Select(x => new VerifiableChange(x).AsVerifiedChange())
                .OfType<AcceptedChange>()
                .Select(change => change.Translate())
                .ToArray()
        };

        return new Event(@event);
    }

    private Event BuildStreetNameEvent(StreetNameWasRenamed message)
    {
        return new Event(new StreetNameRenamed
        {
            StreetNameLocalId = message.PersistentLocalId,
            DestinationStreetNameLocalId = message.DestinationPersistentLocalId
        });
    }
}
