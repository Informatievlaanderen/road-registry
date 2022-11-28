namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Schema;

    public class RoadNodeRecordProjection : ConnectedProjection<ProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadNodeRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var translate = GeometryTranslator.Translate(envelope.Message.Geometry);
                var roadNode = await context.RoadNodes.AddAsync(
                    new RoadNodeRecord(
                        envelope.Message.Id,
                        envelope.Message.Type,
                        translate,
                        envelope.Message.Origin.Since,
                        envelope.Message.Origin.Organization,
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
                            await RemoveRoadNode(roadNodeRemoved, context);
                            break;
                    }
            });
        }

        private async Task AddRoadNode(ProducerSnapshotContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded roadNodeAdded,
            CancellationToken token)
        {
            var roadNode = await context.RoadNodes.AddAsync(new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Type,
                GeometryTranslator.Translate(roadNodeAdded.Geometry),
                LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                envelope.Message.Organization,
                envelope.CreatedUtc), token);

            await Produce(roadNodeAdded.Id, roadNode.Entity.ToContract(), token);
        }

        private async Task ModifyRoadNode(ProducerSnapshotContext context, Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeModified roadNodeModified,
            CancellationToken token)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeModified.Id, cancellationToken: token).ConfigureAwait(false);

            if (roadNodeRecord == null) throw new InvalidOperationException($"RoadNodeRecord with id {roadNodeModified.Id} is not found!");

            roadNodeRecord.Id = roadNodeModified.Id;
            roadNodeRecord.Type = roadNodeModified.Type;
            roadNodeRecord.Geometry = GeometryTranslator.Translate(roadNodeModified.Geometry);
            roadNodeRecord.Origin.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadNodeRecord.Origin.Organization = envelope.Message.Organization;
            roadNodeRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(roadNodeRecord.Id, roadNodeRecord.ToContract(), token);
        }

        private async Task RemoveRoadNode(RoadNodeRemoved roadNodeRemoved, ProducerSnapshotContext context)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeRemoved.Id).ConfigureAwait(false);

            if (roadNodeRecord == null) return;

            context.RoadNodes.Remove(roadNodeRecord);
            var result = await _kafkaProducer.Produce(
                roadNodeRecord.Id.ToString(CultureInfo.InvariantCulture),
                "{}",
                CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
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
