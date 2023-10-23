namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Abstractions;
    using BackOffice.Extensions;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class RoadSegmentRecordProjection : ConnectedProjection<RoadSegmentProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadSegmentRecordProjection(IKafkaProducer kafkaProducer, IStreetNameCache streetNameCache)
        {
            _kafkaProducer = kafkaProducer;

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var status = RoadSegmentStatus.Parse(envelope.Message.Status);
                var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var category = RoadSegmentCategory.Parse(envelope.Message.Category);
                var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

                var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
                var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.LeftSide.StreetNameId, token);
                var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, envelope.Message.RightSide.StreetNameId, token);

                var roadNode = await context.RoadSegments.AddAsync(
                    new RoadSegmentRecord
                    {
                        Id = envelope.Message.Id,
                        Version = envelope.Message.Version,

                        MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                        MaintainerName = envelope.Message.MaintenanceAuthority.Name,

                        MethodId = method.Translation.Identifier,
                        MethodDutchName = method.Translation.Name,

                        CategoryId = category.Translation.Identifier,
                        CategoryDutchName = category.Translation.Name,

                        Geometry = GeometryTranslator.Translate(envelope.Message.Geometry),
                        GeometryVersion = envelope.Message.GeometryVersion,

                        MorphologyId = morphology.Translation.Identifier,
                        MorphologyDutchName = morphology.Translation.Name,

                        StatusId = status.Translation.Identifier,
                        StatusDutchName = status.Translation.Name,

                        AccessRestrictionId = accessRestriction.Translation.Identifier,
                        AccessRestrictionDutchName = accessRestriction.Translation.Name,

                        RecordingDate = envelope.Message.RecordingDate,
                        TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32(),

                        LeftSideMunicipalityId = null,
                        LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode ??
                                                      envelope.Message.LeftSide.MunicipalityNISCode,
                        LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                        LeftSideStreetName = leftSideStreetNameRecord?.Name ??
                                             envelope.Message.LeftSide.StreetName,
                        RightSideMunicipalityId = null,
                        RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode ??
                                                       envelope.Message.RightSide.MunicipalityNISCode,
                        RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                        RightSideStreetName = rightSideStreetNameRecord?.Name ??
                                              envelope.Message.RightSide.StreetName,

                        RoadSegmentVersion = envelope.Message.Version,

                        BeginRoadNodeId = envelope.Message.StartNodeId,
                        EndRoadNodeId = envelope.Message.EndNodeId,
                        StreetNameCachePosition = streetNameCachePosition,

                        Origin = envelope.Message.Origin.ToOrigin(),
                        LastChangedTimestamp = envelope.CreatedUtc
                    });

                await Produce(envelope.Message.Id, roadNode.Entity.ToContract(), token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                    switch (change)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            await AddRoadSegment(streetNameCache, context, envelope, roadSegmentAdded, token);
                            break;

                        case RoadSegmentModified roadSegmentModified:
                            await ModifyRoadSegment(streetNameCache, context, envelope, roadSegmentModified, token);
                            break;

                        case RoadSegmentAttributesModified roadSegmentAttributesModified:
                            await ModifyRoadSegmentAttributes(streetNameCache, context, envelope, roadSegmentAttributesModified, token);
                            break;

                        case RoadSegmentGeometryModified roadSegmentGeometryModified:
                            await ModifyRoadSegmentGeometry(streetNameCache, context, envelope, roadSegmentGeometryModified, token);
                            break;

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await RemoveRoadSegment(roadSegmentRemoved, context, envelope, token);
                            break;
                    }
            });

            When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
            {
                await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
            });

            When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
            {
                if (envelope.Message.NameModified)
                {
                    await RenameOrganization(context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
                }
            });
        }

        private async Task AddRoadSegment(IStreetNameCache streetNameCache,
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAdded roadSegmentAdded,
            CancellationToken token)
        {
            var removedRecord = context.RoadSegments.Local.SingleOrDefault(x => x.Id == roadSegmentAdded.Id && x.IsRemoved)
                ?? await context.RoadSegments.SingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id && x.IsRemoved, token);
            if (removedRecord is not null)
            {
                context.RoadSegments.Remove(removedRecord);
            }

            var transactionId = new TransactionId(envelope.Message.TransactionId);

            var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);

            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);

            var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);

            var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);

            var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentAdded.RightSide.StreetNameId, token);

            var roadSegmentRecord = new RoadSegmentRecord
            {
                Id = roadSegmentAdded.Id,
                Version = roadSegmentAdded.Version,

                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = roadSegmentAdded.MaintenanceAuthority.Name,

                MethodId = method.Translation.Identifier,
                MethodDutchName = method.Translation.Name,

                CategoryId = category.Translation.Identifier,
                CategoryDutchName = category.Translation.Name,

                Geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry),
                GeometryVersion = roadSegmentAdded.GeometryVersion,

                MorphologyId = morphology.Translation.Identifier,
                MorphologyDutchName = morphology.Translation.Name,

                StatusId = status.Translation.Identifier,
                StatusDutchName = status.Translation.Name,

                AccessRestrictionId = accessRestriction.Translation.Identifier,
                AccessRestrictionDutchName = accessRestriction.Translation.Name,

                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32(),

                LeftSideMunicipalityId = null,
                LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                LeftSideStreetName = leftSideStreetNameRecord?.Name,

                RightSideMunicipalityId = null,
                RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                RightSideStreetName = rightSideStreetNameRecord?.Name,

                RoadSegmentVersion = roadSegmentAdded.Version,

                BeginRoadNodeId = roadSegmentAdded.StartNodeId,
                EndRoadNodeId = roadSegmentAdded.EndNodeId,
                StreetNameCachePosition = streetNameCachePosition,

                Origin = envelope.Message.ToOrigin(),
                LastChangedTimestamp = envelope.CreatedUtc
            };

            await context.RoadSegments.AddAsync(roadSegmentRecord, token);
            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task ModifyRoadSegment(IStreetNameCache streetNameCache,
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentModified roadSegmentModified,
            CancellationToken token)
        {
            var transactionId = new TransactionId(envelope.Message.TransactionId);

            var method =
                RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);

            var accessRestriction =
                RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);

            var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);

            var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);

            var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, roadSegmentModified.RightSide.StreetNameId, token);

            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id, cancellationToken: token).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found");
            }

            roadSegmentRecord.Version = roadSegmentModified.Version;
            roadSegmentRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
            roadSegmentRecord.MaintainerName = roadSegmentModified.MaintenanceAuthority.Name;

            roadSegmentRecord.MethodId = method.Translation.Identifier;
            roadSegmentRecord.MethodDutchName = method.Translation.Name;

            roadSegmentRecord.CategoryId = category.Translation.Identifier;
            roadSegmentRecord.CategoryDutchName = category.Translation.Name;

            roadSegmentRecord.Geometry = GeometryTranslator.Translate(roadSegmentModified.Geometry);
            roadSegmentRecord.GeometryVersion = roadSegmentModified.GeometryVersion;

            roadSegmentRecord.MorphologyId = morphology.Translation.Identifier;
            roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;

            roadSegmentRecord.StatusId = status.Translation.Identifier;
            roadSegmentRecord.StatusDutchName = status.Translation.Name;

            roadSegmentRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            roadSegmentRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

            roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            roadSegmentRecord.LeftSideMunicipalityId = null;
            roadSegmentRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
            roadSegmentRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
            roadSegmentRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

            roadSegmentRecord.RightSideMunicipalityId = null;
            roadSegmentRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
            roadSegmentRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
            roadSegmentRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

            roadSegmentRecord.RoadSegmentVersion = roadSegmentModified.Version;

            roadSegmentRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
            roadSegmentRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
            roadSegmentRecord.StreetNameCachePosition = streetNameCachePosition;

            roadSegmentRecord.Origin = envelope.Message.ToOrigin();
            roadSegmentRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task ModifyRoadSegmentAttributes(IStreetNameCache streetNameCache,
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAttributesModified roadSegmentAttributesModified,
            CancellationToken token)
        {
            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentAttributesModified.Id, cancellationToken: token).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
            }

            if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
            {
                roadSegmentRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
                roadSegmentRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
            }

            if (roadSegmentAttributesModified.Category is not null)
            {
                var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

                roadSegmentRecord.CategoryId = category.Translation.Identifier;
                roadSegmentRecord.CategoryDutchName = category.Translation.Name;
            }

            if (roadSegmentAttributesModified.Morphology is not null)
            {
                var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

                roadSegmentRecord.MorphologyId = morphology.Translation.Identifier;
                roadSegmentRecord.MorphologyDutchName = morphology.Translation.Name;
            }

            if (roadSegmentAttributesModified.Status is not null)
            {
                var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

                roadSegmentRecord.StatusId = status.Translation.Identifier;
                roadSegmentRecord.StatusDutchName = status.Translation.Name;
            }

            if (roadSegmentAttributesModified.AccessRestriction is not null)
            {
                var accessRestriction =
                    RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

                roadSegmentRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
                roadSegmentRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;
            }

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            roadSegmentRecord.Version = roadSegmentAttributesModified.Version;
            roadSegmentRecord.RoadSegmentVersion = roadSegmentAttributesModified.Version;

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            roadSegmentRecord.StreetNameCachePosition = streetNameCachePosition;

            roadSegmentRecord.Origin = envelope.Message.ToOrigin();
            roadSegmentRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task ModifyRoadSegmentGeometry(IStreetNameCache streetNameCache,
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentGeometryModified segment,
            CancellationToken token)
        {
            var roadSegmentRecord = await context.RoadSegments.FindAsync(segment.Id, cancellationToken: token).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
            }

            roadSegmentRecord.Geometry = GeometryTranslator.Translate(segment.Geometry);
            roadSegmentRecord.GeometryVersion = segment.GeometryVersion;

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            roadSegmentRecord.Version = segment.Version;
            roadSegmentRecord.RoadSegmentVersion = segment.Version;

            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
            roadSegmentRecord.StreetNameCachePosition = streetNameCachePosition;

            roadSegmentRecord.Origin = envelope.Message.ToOrigin();
            roadSegmentRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task RemoveRoadSegment(RoadSegmentRemoved roadSegmentRemoved, RoadSegmentProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken token)
        {
            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id, cancellationToken: token).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentRemoved.Id} is not found");
            }
            if (roadSegmentRecord.IsRemoved)
            {
                return;
            }

            roadSegmentRecord.Origin = envelope.Message.ToOrigin();
            roadSegmentRecord.LastChangedTimestamp = envelope.CreatedUtc;
            roadSegmentRecord.IsRemoved = true;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task RenameOrganization(
            RoadSegmentProducerSnapshotContext context,
            OrganizationId organizationId,
            OrganizationName organizationName,
            CancellationToken cancellationToken)
        {
            await context.RoadSegments
                .ForEachBatchAsync(q => q
                    .Where(x => x.MaintainerId == organizationId), 5000, async dbRecords =>
                {
                    foreach (var dbRecord in dbRecords)
                    {
                        if (dbRecord.MaintainerId == organizationId)
                        {
                            dbRecord.MaintainerName = organizationName;
                        }

                        await Produce(dbRecord.Id, dbRecord.ToContract(), cancellationToken);
                    }
                }, cancellationToken);
        }

        private async Task Produce(int roadSegmentId, RoadSegmentSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await _kafkaProducer.Produce(
                roadSegmentId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }

        private static async Task<StreetNameCacheItem> TryGetFromCache(
            IStreetNameCache streetNameCache,
            int? streetNameId,
            CancellationToken token)
        {
            return streetNameId.HasValue ? await streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) : null;
        }
    }
}
