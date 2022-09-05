namespace RoadRegistry.Wfs.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class RoadNodeRecordProjection : ConnectedProjection<WfsContext>
    {
        public RoadNodeRecordProjection()
        {
            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                await context.RoadNodes.AddAsync(new RoadNodeRecord
                {
                    Id = envelope.Message.Id,
                    BeginTime = envelope.Message.Origin.Since,
                    Type = envelope.Message.Type
                }, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {

                foreach (var change in envelope.Message.Changes.Flatten())
                {
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
                }
            });
        }

        private static async Task AddRoadNode(WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded roadNodeAdded,
            CancellationToken token)
        {
            await context.RoadNodes.AddAsync(new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                Type = roadNodeAdded.Type
            }, token);
        }

        private static async Task ModifyRoadNode(WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeModified roadNodeModified,
            CancellationToken token)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeModified.Id, token).ConfigureAwait(false);

            if (roadNodeRecord == null)
            {
                throw new InvalidOperationException($"RoadNodeRecord with id {roadNodeModified.Id} is not found!");
            }

            roadNodeRecord.Id = roadNodeModified.Id;
            roadNodeRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadNodeRecord.Type = roadNodeModified.Type;
        }

        private static async Task RemoveRoadNode(RoadNodeRemoved roadNodeRemoved, WfsContext context)
        {
            var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeRemoved.Id).ConfigureAwait(false);

            if (roadNodeRecord == null)
            {
                return;
            }

            context.RoadNodes.Remove(roadNodeRecord);
        }

    }
}
