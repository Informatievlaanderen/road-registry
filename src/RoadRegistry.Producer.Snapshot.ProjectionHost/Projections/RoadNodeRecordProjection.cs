namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class RoadNodeRecordProjection : ConnectedProjection<ProducerSnapshotContext>
    {
        public RoadNodeRecordProjection()
        {
            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                await context.RoadNodes.AddAsync(
                    new RoadNodeRecord(
                        envelope.Message.Id,
                        envelope.Message.Type,
                        GeometryTranslator.Translate(envelope.Message.Geometry),
                        envelope.Message.Origin.Since,
                        envelope.Message.Origin.Organization,
                        envelope.CreatedUtc)
                    , token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
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

        private static async Task AddRoadNode(ProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded roadNodeAdded,
            CancellationToken token)
        {
            await context.RoadNodes.AddAsync(new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Type,
                GeometryTranslator.Translate(roadNodeAdded.Geometry),
                LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                envelope.Message.Organization,
                envelope.CreatedUtc), token);
        }

        private static async Task ModifyRoadNode(ProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
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
        }

        private static async Task RemoveRoadNode(RoadNodeRemoved roadNodeRemoved, ProducerSnapshotContext context)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeRemoved.Id).ConfigureAwait(false);

            if (roadNodeRecord == null) return;

            context.RoadNodes.Remove(roadNodeRecord);
        }
    }
}
