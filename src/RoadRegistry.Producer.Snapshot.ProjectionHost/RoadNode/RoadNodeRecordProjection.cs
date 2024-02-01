namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using BackOffice;
    using BackOffice.Extensions;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Projections;
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    public class RoadNodeRecordProjection : ConnectedProjection<RoadNodeProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadNodeRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var roadNodeAdded = envelope.Message;

                var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;

                var roadNode = await context.RoadNodes.AddAsync(
                    new RoadNodeRecord
                    {
                        Id = roadNodeAdded.Id,
                        Version = roadNodeAdded.Version,
                        TypeId = typeTranslation.Identifier,
                        TypeDutchName = typeTranslation.Name,
                        Geometry = GeometryTranslator.Translate(roadNodeAdded.Geometry),
                        Origin = envelope.Message.Origin.ToOrigin(),
                        LastChangedTimestamp = envelope.CreatedUtc
                    }, token);

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

        private async Task AddRoadNode(RoadNodeProducerSnapshotContext context,
            Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded roadNodeAdded,
            CancellationToken token)
        {
            var dbRecord = await context.RoadNodes
                .FindAsync(x => x.Id == roadNodeAdded.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                dbRecord = new RoadNodeRecord
                {
                    Id = roadNodeAdded.Id
                };
                await context.RoadNodes.AddAsync(dbRecord, token);
            }
            else
            {
                dbRecord.IsRemoved = false;
            }

            var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;

            dbRecord.Version = roadNodeAdded.Version;
            dbRecord.TypeId = typeTranslation.Identifier;
            dbRecord.TypeDutchName = typeTranslation.Name;
            dbRecord.Geometry = GeometryTranslator.Translate(roadNodeAdded.Geometry);
            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task ModifyRoadNode(
            RoadNodeProducerSnapshotContext context,
            Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeModified roadNodeModified,
            CancellationToken token)
        {
            var dbRecord = await context.RoadNodes
                .FindAsync(x => x.Id == roadNodeModified.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"{nameof(RoadNodeRecord)} with id {roadNodeModified.Id} is not found");
            }

            var typeTranslation = RoadNodeType.Parse(roadNodeModified.Type).Translation;

            dbRecord.Id = roadNodeModified.Id;
            dbRecord.Version = roadNodeModified.Version;
            dbRecord.TypeId = typeTranslation.Identifier;
            dbRecord.TypeDutchName = typeTranslation.Name;
            dbRecord.Geometry = GeometryTranslator.Translate(roadNodeModified.Geometry);
            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task RemoveRoadNode(
            RoadNodeProducerSnapshotContext context,
            Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeRemoved roadNodeRemoved,
            CancellationToken token)
        {
            var dbRecord = await context.RoadNodes
                .FindAsync(x => x.Id == roadNodeRemoved.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadNodeRecord with id {roadNodeRemoved.Id} is not found");
            }
            if (dbRecord.IsRemoved)
            {
                return;
            }

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;
            dbRecord.IsRemoved = true;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
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
