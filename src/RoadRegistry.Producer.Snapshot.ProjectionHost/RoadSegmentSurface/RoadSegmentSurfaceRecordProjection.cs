namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
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
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class RoadSegmentSurfaceRecordProjection : ConnectedProjection<RoadSegmentSurfaceProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadSegmentSurfaceRecordProjection(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;

            When<Envelope<ImportedRoadSegment>>(ImportedRoadSegment);
            When<Envelope<RoadNetworkChangesAccepted>>(RoadNetworkChangesAccepted);
        }

        private async Task ImportedRoadSegment(RoadSegmentSurfaceProducerSnapshotContext context, Envelope<ImportedRoadSegment> envelope, CancellationToken token)
        {
            if (envelope.Message.Surfaces.Length == 0)
            {
                return;
            }

            var surfaces = envelope.Message
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;

                    return new RoadSegmentSurfaceRecord(
                        surface.AttributeId,
                        envelope.Message.Id,
                        surface.AsOfGeometryVersion,
                        typeTranslation.Identifier,
                        typeTranslation.Name,
                        (double)surface.FromPosition,
                        (double)surface.ToPosition,
                        surface.Origin.ToOrigin(),
                        envelope.CreatedUtc
                    );
                });

            foreach (var surface in surfaces)
            {
                var roadSegmentSurfaceRecord = await context.RoadSegmentSurfaces.AddAsync(surface, token);

                await Produce(roadSegmentSurfaceRecord.Entity.Id, roadSegmentSurfaceRecord.Entity.ToContract(), token);
            }
        }

        private async Task RoadNetworkChangesAccepted(RoadSegmentSurfaceProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken token)
        {
            foreach (var change in envelope.Message.Changes.Flatten())
            {
                switch (change)
                {
                    case RoadSegmentAdded roadSegmentSurface:
                        await RoadSegmentAdded(context, envelope, roadSegmentSurface, token);
                        break;
                    case RoadSegmentModified roadSegmentSurface:
                        await RoadSegmentModified(context, envelope, roadSegmentSurface, token);
                        break;
                    case RoadSegmentRemoved roadSegmentSurface:
                        await RoadSegmentRemoved(context, envelope, roadSegmentSurface, token);
                        break;
                }
            }
        }

        private async Task RoadSegmentAdded(RoadSegmentSurfaceProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAdded roadSegmentAdded,
            CancellationToken token)
        {
            if (roadSegmentAdded.Surfaces.Length == 0)
            {
                return;
            }

            var surfaces = roadSegmentAdded
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;

                    return new RoadSegmentSurfaceRecord(
                        surface.AttributeId,
                        roadSegmentAdded.Id,
                        surface.AsOfGeometryVersion,
                        typeTranslation.Identifier,
                        typeTranslation.Name,
                        (double)surface.FromPosition,
                        (double)surface.ToPosition,
                        envelope.Message.ToOrigin(),
                        envelope.CreatedUtc
                    );
                });

            foreach (var surface in surfaces)
            {
                var roadSegmentSurfaceRecord = await context.RoadSegmentSurfaces.AddAsync(surface, token);

                await Produce(roadSegmentSurfaceRecord.Entity.Id, roadSegmentSurfaceRecord.Entity.ToContract(), token);
            }
        }
        //TODO-rik add handler for RoadSegmentAttributesModified
        private async Task RoadSegmentModified(
            RoadSegmentSurfaceProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentModified roadSegmentModified,
            CancellationToken token)
        {
            if (roadSegmentModified.Lanes.Length == 0)
            {
                var roadSegmentSurfaceRecords = context
                    .RoadSegmentSurfaces
                    .Local.Where(a => a.RoadSegmentId == roadSegmentModified.Id)
                    .Concat(await context
                        .RoadSegmentSurfaces
                        .Where(a => a.RoadSegmentId == roadSegmentModified.Id)
                        .ToArrayAsync(token)
                        .ConfigureAwait(false)
                    );

                foreach (var roadSegmentSurfaceRecord in roadSegmentSurfaceRecords)
                {
                    MarkRoadSegmentSurfaceAsRemoved(roadSegmentSurfaceRecord, envelope);

                    await Produce(roadSegmentSurfaceRecord.Id, roadSegmentSurfaceRecord.ToContract(), token);
                }
            }
            else
            {
                //Causes all attributes to be loaded into Local
                await context
                    .RoadSegmentSurfaces
                    .Where(a => a.RoadSegmentId == roadSegmentModified.Id)
                    .ToArrayAsync(token)
                    .ConfigureAwait(false);
                var currentSet = context
                    .RoadSegmentSurfaces
                    .Local.Where(a => a.RoadSegmentId == roadSegmentModified.Id)
                    .ToDictionary(a => a.Id);

                var nextSet = roadSegmentModified
                    .Surfaces
                    .Select(surface =>
                    {
                        var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;

                        return new RoadSegmentSurfaceRecord(
                            surface.AttributeId,
                            roadSegmentModified.Id,
                            surface.AsOfGeometryVersion,
                            typeTranslation.Identifier,
                            typeTranslation.Name,
                            (double)surface.FromPosition,
                            (double)surface.ToPosition,
                            envelope.Message.ToOrigin(),
                            envelope.CreatedUtc
                        );
                    })
                    .ToDictionary(a => a.Id);

                var roadSegmentSurfaceRecords = context.RoadSegmentSurfaces.Synchronize(currentSet, nextSet,
                    (current, next) =>
                    {
                        current.RoadSegmentId = next.RoadSegmentId;
                        current.RoadSegmentGeometryVersion = next.RoadSegmentGeometryVersion;
                        current.TypeId = next.TypeId;
                        current.TypeDutchName = next.TypeDutchName;
                        current.FromPosition = next.FromPosition;
                        current.ToPosition = next.ToPosition;
                        current.Origin = next.Origin;
                        current.LastChangedTimestamp = next.LastChangedTimestamp;
                        current.IsRemoved = next.IsRemoved;
                    },
                    current =>
                    {
                        MarkRoadSegmentSurfaceAsRemoved(current, envelope);
                    });

                foreach (var roadSegmentSurfaceRecord in roadSegmentSurfaceRecords)
                {
                    await Produce(roadSegmentSurfaceRecord.Id, roadSegmentSurfaceRecord.ToContract(), token);
                }
            }
        }

        private async Task RoadSegmentRemoved(
            RoadSegmentSurfaceProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentRemoved roadSegmentRemoved,
            CancellationToken token)
        {
            var roadSegmentSurfaceRecords = context
                .RoadSegmentSurfaces
                .Local.Where(a => a.RoadSegmentId == roadSegmentRemoved.Id)
                .Concat(await context
                    .RoadSegmentSurfaces
                    .Where(a => a.RoadSegmentId == roadSegmentRemoved.Id)
                    .ToArrayAsync(token)
                    .ConfigureAwait(false)
                );

            foreach (var roadSegmentSurfaceRecord in roadSegmentSurfaceRecords)
            {
                MarkRoadSegmentSurfaceAsRemoved(roadSegmentSurfaceRecord, envelope);

                await Produce(roadSegmentSurfaceRecord.Id, roadSegmentSurfaceRecord.ToContract(), token);
            }
        }

        private static void MarkRoadSegmentSurfaceAsRemoved(RoadSegmentSurfaceRecord roadSegmentSurfaceRecord, Envelope<RoadNetworkChangesAccepted> envelope)
        {
            roadSegmentSurfaceRecord.Origin = envelope.Message.ToOrigin();
            roadSegmentSurfaceRecord.LastChangedTimestamp = envelope.CreatedUtc;
            roadSegmentSurfaceRecord.IsRemoved = true;
        }

        private async Task Produce(int roadSegmentSurfaceId, RoadSegmentSurfaceSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await _kafkaProducer.Produce(
                roadSegmentSurfaceId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
