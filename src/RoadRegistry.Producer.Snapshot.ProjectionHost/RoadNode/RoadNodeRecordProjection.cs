namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class RoadNodeRecordProjection : ConnectedProjection<RoadNodeProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadNodeRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var geometry = GeometryTranslator.Translate(envelope.Message.Geometry);
                var typeTranslation = RoadNodeType.Parse(envelope.Message.Type).Translation;

                var roadNode = await context.RoadNodes.AddAsync(
                    new RoadNodeRecord(
                        envelope.Message.Id,
                        envelope.Message.Version,
                        typeTranslation.Identifier,
                        typeTranslation.Name,
                        geometry,
                        envelope.Message.Origin.ToOrigin(),
                        envelope.CreatedUtc)
                    , token);

                await Produce(envelope.Message.Id, roadNode.Entity.ToContract(), token);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                    switch (change)
                    {
                        case RoadNodeAdded roadNodeAdded:
                            await AddRoadNode(context, envelope, roadNodeAdded, token);
                            break;

                        case RoadNodeModified roadNodeModified:
                            await ModifyRoadNode(context, envelope, roadNodeModified, token);
                            break;

                        case RoadNodeRemoved roadNodeRemoved:
                            await RemoveRoadNode(context, envelope, roadNodeRemoved, token);
                            break;
                    }
            });
        }

        private async Task AddRoadNode(RoadNodeProducerSnapshotContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded roadNodeAdded,
            CancellationToken token)
        {
            var removedRecord = context.RoadNodes.Local.SingleOrDefault(x => x.Id == roadNodeAdded.Id && x.IsRemoved)
                ?? await context.RoadNodes.SingleOrDefaultAsync(x => x.Id == roadNodeAdded.Id && x.IsRemoved, token);
            if (removedRecord is not null)
            {
                context.RoadNodes.Remove(removedRecord);
            }

            var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;

            var roadNode = await context.RoadNodes.AddAsync(new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Version,
                typeTranslation.Identifier,
                typeTranslation.Name,
                GeometryTranslator.Translate(roadNodeAdded.Geometry),
                envelope.Message.ToOrigin(),
                envelope.CreatedUtc), token);

            await Produce(roadNodeAdded.Id, roadNode.Entity.ToContract(), token);
        }

        private async Task ModifyRoadNode(
            RoadNodeProducerSnapshotContext context,
            Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeModified roadNodeModified,
            CancellationToken token)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeModified.Id, cancellationToken: token).ConfigureAwait(false);

            if (roadNodeRecord == null)
            {
                throw new InvalidOperationException($"{nameof(RoadNodeRecord)} with id {roadNodeModified.Id} is not found");
            }

            var typeTranslation = RoadNodeType.Parse(roadNodeModified.Type).Translation;

            roadNodeRecord.Id = roadNodeModified.Id;
            roadNodeRecord.Version = roadNodeModified.Version;
            roadNodeRecord.TypeId = typeTranslation.Identifier;
            roadNodeRecord.TypeDutchName = typeTranslation.Name;
            roadNodeRecord.Geometry = GeometryTranslator.Translate(roadNodeModified.Geometry);
            roadNodeRecord.Origin = envelope.Message.ToOrigin();
            roadNodeRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(roadNodeRecord.Id, roadNodeRecord.ToContract(), token);
        }

        private async Task RemoveRoadNode(
            RoadNodeProducerSnapshotContext context,
            Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeRemoved roadNodeRemoved,
            CancellationToken token)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeRemoved.Id, cancellationToken: token).ConfigureAwait(false);
            if (roadNodeRecord == null)
            {
                throw new InvalidOperationException($"RoadNodeRecord with id {roadNodeRemoved.Id} is not found");
            }
            if (roadNodeRecord.IsRemoved)
            {
                return;
            }

            roadNodeRecord.Origin = envelope.Message.ToOrigin();
            roadNodeRecord.LastChangedTimestamp = envelope.CreatedUtc;
            roadNodeRecord.IsRemoved = true;

            await Produce(roadNodeRecord.Id, roadNodeRecord.ToContract(), token);
        }

        private async Task Produce(int roadNodeId, RoadNodeSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await _kafkaProducer.Produce(
                roadNodeId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
