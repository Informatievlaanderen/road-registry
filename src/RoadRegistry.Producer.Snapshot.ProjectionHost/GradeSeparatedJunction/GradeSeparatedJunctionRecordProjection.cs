namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using BackOffice;
    using BackOffice.Extensions;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using Projections;
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<GradeSeparatedJunctionProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public GradeSeparatedJunctionRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;

            When<Envelope<ImportedGradeSeparatedJunction>>(ImportedGradeSeparatedJunction);
            When<Envelope<RoadNetworkChangesAccepted>>(RoadNetworkChangesAccepted);
        }

        private async Task ImportedGradeSeparatedJunction(GradeSeparatedJunctionProducerSnapshotContext context, Envelope<ImportedGradeSeparatedJunction> envelope, CancellationToken token)
        {
            var gradeSeparatedJunctionAdded = envelope.Message;

            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation;

            var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.AddAsync(
                new GradeSeparatedJunctionRecord
                {
                    Id = gradeSeparatedJunctionAdded.Id,
                    LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    TypeId = typeTranslation.Identifier,
                    TypeDutchName = typeTranslation.Name,
                    Origin = envelope.Message.Origin.ToOrigin(),
                    LastChangedTimestamp = envelope.CreatedUtc
                }, token);

            await Produce(gradeSeparatedJunctionRecord.Entity.Id, gradeSeparatedJunctionRecord.Entity.ToContract(), token);
        }

        private async Task RoadNetworkChangesAccepted(GradeSeparatedJunctionProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken token)
        {
            foreach (var change in envelope.Message.Changes.Flatten())
            {
                switch (change)
                {
                    case GradeSeparatedJunctionAdded gradeSeparatedJunction:
                        await GradeSeparatedJunctionAdded(context, envelope, gradeSeparatedJunction, token);
                        break;
                    case GradeSeparatedJunctionModified gradeSeparatedJunction:
                        await GradeSeparatedJunctionModified(context, envelope, gradeSeparatedJunction, token);
                        break;
                    case GradeSeparatedJunctionRemoved gradeSeparatedJunction:
                        await GradeSeparatedJunctionRemoved(context, envelope, gradeSeparatedJunction, token);
                        break;
                }
            }
        }

        private async Task GradeSeparatedJunctionAdded(GradeSeparatedJunctionProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope,
            GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded,
            CancellationToken token)
        {
            var dbRecord = await context.GradeSeparatedJunctions
                .SingleOrDefaultIncludingLocalAsync(x => x.Id == gradeSeparatedJunctionAdded.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                dbRecord = new GradeSeparatedJunctionRecord();
                await context.GradeSeparatedJunctions.AddAsync(dbRecord, token);
            }
            else
            {
                dbRecord.IsRemoved = false;
            }

            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation;

            dbRecord.Id = gradeSeparatedJunctionAdded.Id;
            dbRecord.LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId;
            dbRecord.UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId;
            dbRecord.TypeId = typeTranslation.Identifier;
            dbRecord.TypeDutchName = typeTranslation.Name;
            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;
            
            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task GradeSeparatedJunctionModified(
            GradeSeparatedJunctionProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            GradeSeparatedJunctionModified gradeSeparatedJunctionModified,
            CancellationToken token)
        {
            var gradeSeparatedJunctionRecord = await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionModified.Id, cancellationToken: token).ConfigureAwait(false);

            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionModified.Type).Translation;

            if (gradeSeparatedJunctionRecord != null)
            {
                gradeSeparatedJunctionRecord.UpperRoadSegmentId = gradeSeparatedJunctionModified.UpperRoadSegmentId;
                gradeSeparatedJunctionRecord.LowerRoadSegmentId = gradeSeparatedJunctionModified.LowerRoadSegmentId;
                gradeSeparatedJunctionRecord.TypeId = typeTranslation.Identifier;
                gradeSeparatedJunctionRecord.TypeDutchName = typeTranslation.Name;
                gradeSeparatedJunctionRecord.Origin = envelope.Message.ToOrigin();
                gradeSeparatedJunctionRecord.LastChangedTimestamp = envelope.CreatedUtc;

                await Produce(gradeSeparatedJunctionRecord.Id, gradeSeparatedJunctionRecord.ToContract(), token);
            }
        }

        private async Task GradeSeparatedJunctionRemoved(
            GradeSeparatedJunctionProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved,
            CancellationToken token)
        {
            var gradeSeparatedJunctionRecord =
                await context.GradeSeparatedJunctions.FindAsync(gradeSeparatedJunctionRemoved.Id, cancellationToken: token).ConfigureAwait(false);

            if (gradeSeparatedJunctionRecord == null)
            {
                throw new InvalidOperationException($"{nameof(GradeSeparatedJunctionRecord)} with id {gradeSeparatedJunctionRemoved.Id} is not found");
            }
            if (gradeSeparatedJunctionRecord.IsRemoved)
            {
                return;
            }

            gradeSeparatedJunctionRecord.Origin = envelope.Message.ToOrigin();
            gradeSeparatedJunctionRecord.LastChangedTimestamp = envelope.CreatedUtc;
            gradeSeparatedJunctionRecord.IsRemoved = true;

            await Produce(gradeSeparatedJunctionRecord.Id, gradeSeparatedJunctionRecord.ToContract(), token);
        }

        private async Task Produce(int gradeSeparatedJunctionId, GradeSeparatedJunctionSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await _kafkaProducer.Produce(
                gradeSeparatedJunctionId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
