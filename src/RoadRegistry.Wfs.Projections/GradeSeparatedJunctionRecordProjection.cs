namespace RoadRegistry.Wfs.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<WfsContext>
    {
        public GradeSeparatedJunctionRecordProjection()
        {
            When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
            {
                await context.GradeSeparatedJunctions.AddAsync(new GradeSeparatedJunctionRecord
                {
                    Id = envelope.Message.Id,
                    BeginTime = envelope.Message.Origin.Since,
                    Type = envelope.Message.Type,
                    LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                    UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId
                }, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {

                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case GradeSeparatedJunctionAdded GradeSeparatedJunctionAdded:
                            await AddGradeSeparatedJunction(context, envelope, GradeSeparatedJunctionAdded, token);
                            break;

                        case GradeSeparatedJunctionModified GradeSeparatedJunctionModified:
                            await ModifyGradeSeparatedJunction(context, envelope, GradeSeparatedJunctionModified, token);
                            break;

                        case GradeSeparatedJunctionRemoved GradeSeparatedJunctionRemoved:
                            await RemoveGradeSeparatedJunction(GradeSeparatedJunctionRemoved, context);
                            break;
                    }
                }
            });
        }

        private static async Task AddGradeSeparatedJunction(WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded,
            CancellationToken token)
        {
            await context.GradeSeparatedJunctions.AddAsync(new GradeSeparatedJunctionRecord
            {
                Id = gradeSeparatedJunctionAdded.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                Type = gradeSeparatedJunctionAdded.Type,
                LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId
            }, token);
        }

        private static async Task ModifyGradeSeparatedJunction(WfsContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            GradeSeparatedJunctionModified gradeSeparatedJunctionModified,
            CancellationToken token)
        {
            var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionModified.Id).ConfigureAwait(false);

            if (gradeSeparatedJunctionRecord == null)
                throw new Exception($"GradeSeparatedJunctionRecord with id {gradeSeparatedJunctionModified.Id} is not found!");

            gradeSeparatedJunctionRecord.Id = gradeSeparatedJunctionModified.Id;
            gradeSeparatedJunctionRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            gradeSeparatedJunctionRecord.Type = gradeSeparatedJunctionModified.Type;
            gradeSeparatedJunctionRecord.LowerRoadSegmentId = gradeSeparatedJunctionModified.LowerRoadSegmentId;
            gradeSeparatedJunctionRecord.UpperRoadSegmentId = gradeSeparatedJunctionModified.UpperRoadSegmentId;
        }

        private static async Task RemoveGradeSeparatedJunction(GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved, WfsContext context)
        {
            var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionRemoved.Id).ConfigureAwait(false);

            if (gradeSeparatedJunctionRecord == null)
            {
                return;
            }

            context.GradeSeparatedJunctions.Remove(gradeSeparatedJunctionRecord);
        }

    }
}
