namespace RoadRegistry.SyncHost.StreetName;

using System;
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
using SqlStreamStore;
using SqlStreamStore.Streams;
using Sync.StreetNameRegistry;
using AcceptedChange = BackOffice.Core.AcceptedChange;
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
        var organization = await editorContext.OrganizationsV2.IncludeLocalSingleOrDefaultAsync(x => x.OvoCode == OrganizationOvoCode.DigitaalVlaanderen, cancellationToken);

        var segmentsPerStreamGrouping = segments
            .GroupBy(x => x.MethodId == RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier
                ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(x.Id))
                : RoadNetworkStreamNameProvider.Default);

        foreach (var segmentsPerStream in segmentsPerStreamGrouping)
        {
            var transactionId = await _roadNetworkIdGenerator.NewTransactionId();

            var requestedChanges = RequestedChanges.Start(transactionId);

            foreach (var roadSegment in segmentsPerStream)
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
                    .SelectMany(change => change.Translate())
                    .ToArray()
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
