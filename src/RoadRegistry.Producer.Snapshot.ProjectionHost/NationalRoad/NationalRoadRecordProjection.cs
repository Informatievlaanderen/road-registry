namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class NationalRoadRecordProjection : ConnectedProjection<NationalRoadProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public NationalRoadRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;

            When<Envelope<ImportedRoadSegment>>(ImportedRoadSegment);
            When<Envelope<RoadNetworkChangesAccepted>>(RoadNetworkChangesAccepted);
        }

        private async Task ImportedRoadSegment(NationalRoadProducerSnapshotContext context, Envelope<ImportedRoadSegment> envelope, CancellationToken token)
        {
            if (envelope.Message.PartOfNationalRoads.Length == 0)
            {
                return;
            }

            var nationalRoads = envelope.Message
                .PartOfNationalRoads
                .Select(nationalRoad => new NationalRoadRecord(
                    nationalRoad.AttributeId,
                    envelope.Message.Id,
                    nationalRoad.Number,
                    nationalRoad.Origin.ToOrigin(),
                    envelope.CreatedUtc)
                );

            foreach (var nationalRoad in nationalRoads)
            {
                var nationalRoadRecord = await context.NationalRoads.AddAsync(nationalRoad, token);

                await Produce(nationalRoadRecord.Entity.Id, nationalRoadRecord.Entity.ToContract(), token);
            }
        }

        private async Task RoadNetworkChangesAccepted(NationalRoadProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken token)
        {
            foreach (var change in envelope.Message.Changes.Flatten())
            {
                switch (change)
                {
                    case RoadSegmentAddedToNationalRoad nationalRoad:
                        await RoadSegmentAddedToNationalRoad(context, envelope, nationalRoad, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad nationalRoad:
                        await RoadSegmentRemovedFromNationalRoad(context, envelope, nationalRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegment:
                        await RoadSegmentRemoved(context, envelope, roadSegment, token);
                        break;
                }
            }
        }

        private async Task RoadSegmentAddedToNationalRoad(NationalRoadProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAddedToNationalRoad nationalRoadAdded,
            CancellationToken token)
        {
            var removedRecord = context.NationalRoads.Local.SingleOrDefault(x => x.Id == nationalRoadAdded.AttributeId && x.IsRemoved)
                ?? await context.NationalRoads.SingleOrDefaultAsync(x => x.Id == nationalRoadAdded.AttributeId && x.IsRemoved, token);
            if (removedRecord is not null)
            {
                context.NationalRoads.Remove(removedRecord);
            }

            var nationalRoad = await context.NationalRoads.AddAsync(new NationalRoadRecord(
                nationalRoadAdded.AttributeId,
                nationalRoadAdded.SegmentId,
                nationalRoadAdded.Number,
                envelope.Message.ToOrigin(),
                envelope.CreatedUtc
            ), token);

            await Produce(nationalRoad.Entity.Id, nationalRoad.Entity.ToContract(), token);
        }

        private async Task RoadSegmentRemovedFromNationalRoad(
            NationalRoadProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentRemovedFromNationalRoad nationalRoadRemoved,
            CancellationToken token)
        {
            await RemovedNationalRoadById(context, envelope, nationalRoadRemoved.AttributeId, token);
        }

        private async Task RoadSegmentRemoved(
            NationalRoadProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentRemoved roadSegment,
            CancellationToken token)
        {
            var nationalRoadRecords =
                context.NationalRoads
                    .Local
                    .Where(x => x.RoadSegmentId == roadSegment.Id)
                    .Concat(context.NationalRoads
                        .Where(x => x.RoadSegmentId == roadSegment.Id));

            foreach (var nationalRoadRecord in nationalRoadRecords)
            {
                await RemovedNationalRoadById(context, envelope, nationalRoadRecord.Id, token);
            }
        }

        private async Task RemovedNationalRoadById(
            NationalRoadProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            int nationalRoadId,
            CancellationToken token)
        {
            var nationalRoadRecord =
                await context.NationalRoads.FindAsync(nationalRoadId, cancellationToken: token).ConfigureAwait(false);

            if (nationalRoadRecord == null)
            {
                throw new InvalidOperationException($"{nameof(NationalRoadRecord)} with id {nationalRoadId} is not found");
            }
            if (nationalRoadRecord.IsRemoved)
            {
                return;
            }

            nationalRoadRecord.Origin = envelope.Message.ToOrigin();
            nationalRoadRecord.LastChangedTimestamp = envelope.CreatedUtc;
            nationalRoadRecord.IsRemoved = true;

            await Produce(nationalRoadRecord.Id, nationalRoadRecord.ToContract(), token);
        }
        
        private async Task Produce(int nationalRoadId, NationalRoadSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await _kafkaProducer.Produce(
                nationalRoadId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
