namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class RoadSegmentRecordProjection : ConnectedProjection<RoadSegmentProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IStreetNameCache _streetNameCache;

        public RoadSegmentRecordProjection(IKafkaProducer kafkaProducer, IStreetNameCache streetNameCache)
        {
            _kafkaProducer = kafkaProducer.ThrowIfNull();
            _streetNameCache = streetNameCache.ThrowIfNull();

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var status = RoadSegmentStatus.Parse(envelope.Message.Status);
                var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var category = RoadSegmentCategory.Parse(envelope.Message.Category);
                var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

                var leftSideStreetNameRecord = await TryGetFromStreetNameCache(envelope.Message.LeftSide.StreetNameId, token);
                var rightSideStreetNameRecord = await TryGetFromStreetNameCache(envelope.Message.RightSide.StreetNameId, token);

                var roadNode = await context.RoadSegments.AddAsync(
                    new RoadSegmentRecord
                    {
                        Id = envelope.Message.Id,
                        Version = envelope.Message.Version,

                        MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                        MaintainerName = OrganizationName.FromValueWithFallback(envelope.Message.MaintenanceAuthority.Name),

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

                        Origin = envelope.Message.Origin.ToOrigin(),
                        LastChangedTimestamp = envelope.CreatedUtc
                    });

                await Produce(envelope.Message.Id, roadNode.Entity.ToContract(), token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var acceptedChange in envelope.Message.Changes.Flatten())
                    switch (acceptedChange)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            await AddRoadSegment(context, envelope, roadSegmentAdded, token);
                            break;

                        case RoadSegmentModified roadSegmentModified:
                            await ModifyRoadSegment(context, envelope, roadSegmentModified, token);
                            break;

                        case RoadSegmentAddedToEuropeanRoad change:
                            await AddRoadSegmentToEuropeanRoad(context, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromEuropeanRoad change:
                            await RemoveRoadSegmentFromEuropeanRoad(context, change, envelope, token);
                            break;

                        case RoadSegmentAddedToNationalRoad change:
                            await AddRoadSegmentToNationalRoad(context, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromNationalRoad change:
                            await RemoveRoadSegmentFromNationalRoad(context, change, envelope, token);
                            break;

                        case RoadSegmentAddedToNumberedRoad change:
                            await AddRoadSegmentToNumberedRoad(context, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromNumberedRoad change:
                            await RemoveRoadSegmentFromNumberedRoad(context, change, envelope, token);
                            break;

                        case RoadSegmentAttributesModified roadSegmentAttributesModified:
                            await ModifyRoadSegmentAttributes(context, envelope, roadSegmentAttributesModified, token);
                            break;

                        case RoadSegmentGeometryModified roadSegmentGeometryModified:
                            await ModifyRoadSegmentGeometry(context, envelope, roadSegmentGeometryModified, token);
                            break;

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await RemoveRoadSegment(context, envelope, roadSegmentRemoved, token);
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

            When<Envelope<StreetNameModified>>(async (context, envelope, token) =>
            {
                if (envelope.Message.NameModified)
                {
                    await UpdateStreetNameLabels(context, new StreetNameLocalId(envelope.Message.Record.PersistentLocalId), envelope.Message.Record.DutchName, token);
                }
            });
        }

        private async Task AddRoadSegment(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAdded roadSegmentAdded,
            CancellationToken token)
        {
            var dbRecord = await context.RoadSegments
                .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                dbRecord = new RoadSegmentRecord
                {
                    Id = roadSegmentAdded.Id
                };
                await context.RoadSegments.AddAsync(dbRecord, token);
            }
            else
            {
                dbRecord.IsRemoved = false;
            }

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);
            var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
            var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);

            var leftSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentAdded.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentAdded.RightSide.StreetNameId, token);

            dbRecord.Version = roadSegmentAdded.Version;

            dbRecord.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name);

            dbRecord.MethodId = method.Translation.Identifier;
            dbRecord.MethodDutchName = method.Translation.Name;

            dbRecord.CategoryId = category.Translation.Identifier;
            dbRecord.CategoryDutchName = category.Translation.Name;

            dbRecord.Geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
            dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;

            dbRecord.MorphologyId = morphology.Translation.Identifier;
            dbRecord.MorphologyDutchName = morphology.Translation.Name;

            dbRecord.StatusId = status.Translation.Identifier;
            dbRecord.StatusDutchName = status.Translation.Name;

            dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

            dbRecord.RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            dbRecord.LeftSideMunicipalityId = null;
            dbRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
            dbRecord.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
            dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

            dbRecord.RightSideMunicipalityId = null;
            dbRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
            dbRecord.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
            dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

            dbRecord.RoadSegmentVersion = roadSegmentAdded.Version;

            dbRecord.BeginRoadNodeId = roadSegmentAdded.StartNodeId;
            dbRecord.EndRoadNodeId = roadSegmentAdded.EndNodeId;

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task ModifyRoadSegment(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentModified roadSegmentModified,
            CancellationToken token)
        {
            var dbRecord = await context.RoadSegments
                .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentModified.Id} is not found");
            }

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);
            var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
            var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
            var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);

            var leftSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentModified.LeftSide.StreetNameId, token);
            var rightSideStreetNameRecord = await TryGetFromStreetNameCache(roadSegmentModified.RightSide.StreetNameId, token);

            dbRecord.Version = roadSegmentModified.Version;
            dbRecord.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
            dbRecord.MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name);

            dbRecord.MethodId = method.Translation.Identifier;
            dbRecord.MethodDutchName = method.Translation.Name;

            dbRecord.CategoryId = category.Translation.Identifier;
            dbRecord.CategoryDutchName = category.Translation.Name;

            dbRecord.Geometry = GeometryTranslator.Translate(roadSegmentModified.Geometry);
            dbRecord.GeometryVersion = roadSegmentModified.GeometryVersion;

            dbRecord.MorphologyId = morphology.Translation.Identifier;
            dbRecord.MorphologyDutchName = morphology.Translation.Name;

            dbRecord.StatusId = status.Translation.Identifier;
            dbRecord.StatusDutchName = status.Translation.Name;

            dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
            dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;

            dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            dbRecord.LeftSideMunicipalityId = null;
            dbRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
            dbRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
            dbRecord.LeftSideStreetName = leftSideStreetNameRecord?.Name;

            dbRecord.RightSideMunicipalityId = null;
            dbRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
            dbRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
            dbRecord.RightSideStreetName = rightSideStreetNameRecord?.Name;

            dbRecord.RoadSegmentVersion = roadSegmentModified.Version;

            dbRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
            dbRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task AddRoadSegmentToEuropeanRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentAddedToEuropeanRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task RemoveRoadSegmentFromEuropeanRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentRemovedFromEuropeanRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task AddRoadSegmentToNationalRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentAddedToNationalRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task RemoveRoadSegmentFromNationalRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentRemovedFromNationalRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task AddRoadSegmentToNumberedRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentAddedToNumberedRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task RemoveRoadSegmentFromNumberedRoad(
            RoadSegmentProducerSnapshotContext context,
            RoadSegmentRemovedFromNumberedRoad change,
            Envelope<RoadNetworkChangesAccepted> envelope,
            CancellationToken token)
        {
            await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
        }

        private async Task ModifyRoadSegmentAttributes(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAttributesModified roadSegmentAttributesModified,
            CancellationToken token)
        {
            var dbRecord = await context.RoadSegments
                .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAttributesModified.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentAttributesModified.Id} is not found");
            }

            if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
            {
                dbRecord.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
                dbRecord.MaintainerName = roadSegmentAttributesModified.MaintenanceAuthority.Name;
            }

            if (roadSegmentAttributesModified.Category is not null)
            {
                var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);

                dbRecord.CategoryId = category.Translation.Identifier;
                dbRecord.CategoryDutchName = category.Translation.Name;
            }

            if (roadSegmentAttributesModified.Morphology is not null)
            {
                var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);

                dbRecord.MorphologyId = morphology.Translation.Identifier;
                dbRecord.MorphologyDutchName = morphology.Translation.Name;
            }

            if (roadSegmentAttributesModified.Status is not null)
            {
                var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);

                dbRecord.StatusId = status.Translation.Identifier;
                dbRecord.StatusDutchName = status.Translation.Name;
            }

            if (roadSegmentAttributesModified.AccessRestriction is not null)
            {
                var accessRestriction =
                    RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);

                dbRecord.AccessRestrictionId = accessRestriction.Translation.Identifier;
                dbRecord.AccessRestrictionDutchName = accessRestriction.Translation.Name;
            }

            if (roadSegmentAttributesModified.LeftSide is not null)
            {
                var streetNameRecord = await TryGetFromStreetNameCache(roadSegmentAttributesModified.LeftSide.StreetNameId, token);

                dbRecord.LeftSideMunicipalityId = null;
                dbRecord.LeftSideMunicipalityNisCode = streetNameRecord?.NisCode;
                dbRecord.LeftSideStreetNameId = roadSegmentAttributesModified.LeftSide.StreetNameId;
                dbRecord.LeftSideStreetName = streetNameRecord?.Name;
            }

            if (roadSegmentAttributesModified.RightSide is not null)
            {
                var streetNameRecord = await TryGetFromStreetNameCache(roadSegmentAttributesModified.RightSide.StreetNameId, token);

                dbRecord.RightSideMunicipalityId = null;
                dbRecord.RightSideMunicipalityNisCode = streetNameRecord?.NisCode;
                dbRecord.RightSideStreetNameId = roadSegmentAttributesModified.RightSide.StreetNameId;
                dbRecord.RightSideStreetName = streetNameRecord?.Name;
            }

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            dbRecord.Version = roadSegmentAttributesModified.Version;
            dbRecord.RoadSegmentVersion = roadSegmentAttributesModified.Version;

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task ModifyRoadSegmentGeometry(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentGeometryModified segment,
            CancellationToken token)
        {
            var dbRecord = await context.RoadSegments
                .IncludeLocalSingleOrDefaultAsync(x => x.Id == segment.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {segment.Id} is not found");
            }

            dbRecord.Geometry = GeometryTranslator.Translate(segment.Geometry);
            dbRecord.GeometryVersion = segment.GeometryVersion;

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            dbRecord.Version = segment.Version;
            dbRecord.RoadSegmentVersion = segment.Version;

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task RemoveRoadSegment(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentRemoved roadSegmentRemoved,
            CancellationToken token
        )
        {
            var dbRecord = await context.RoadSegments
                .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentRemoved.Id, token)
                .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {roadSegmentRemoved.Id} is not found");
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

        private async Task UpdateRoadSegmentVersion(
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            int segmentId,
            int? segmentVersion,
            CancellationToken token)
        {
            if (segmentVersion is null)
            {
                return;
            }

            var dbRecord = await context.RoadSegments
                            .IncludeLocalSingleOrDefaultAsync(x => x.Id == segmentId, token)
                            .ConfigureAwait(false);
            if (dbRecord is null)
            {
                throw new InvalidOperationException($"RoadSegmentRecord with id {segmentId} is not found");
            }

            dbRecord.Version = segmentVersion.Value;
            dbRecord.RoadSegmentVersion = segmentVersion.Value;

            var transactionId = new TransactionId(envelope.Message.TransactionId);
            dbRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            dbRecord.Origin = envelope.Message.ToOrigin();
            dbRecord.LastChangedTimestamp = envelope.CreatedUtc;

            await Produce(dbRecord.Id, dbRecord.ToContract(), token);
        }

        private async Task RenameOrganization(
            RoadSegmentProducerSnapshotContext context,
            OrganizationId organizationId,
            OrganizationName organizationName,
            CancellationToken cancellationToken)
        {
            await context.RoadSegments.IncludeLocalForEachBatchAsync(q =>
                q.Where(x => x.MaintainerId == organizationId),
                5000,
                async dbRecords =>
                {
                    foreach (var dbRecord in dbRecords)
                    {
                        dbRecord.MaintainerName = organizationName;

                        await Produce(dbRecord.Id, dbRecord.ToContract(), cancellationToken);
                    }
                }, cancellationToken);
        }

        private async Task UpdateStreetNameLabels(
            RoadSegmentProducerSnapshotContext context,
            StreetNameLocalId streetNameLocalId,
            string dutchName,
            CancellationToken cancellationToken)
        {
            await context.RoadSegments.IncludeLocalForEachBatchAsync(q =>
                q.Where(x => x.LeftSideStreetNameId == streetNameLocalId || x.RightSideStreetNameId == streetNameLocalId),
                5000,
                async dbRecords =>
                {
                    foreach (var dbRecord in dbRecords)
                    {
                        if (dbRecord.LeftSideStreetNameId == streetNameLocalId)
                        {
                            dbRecord.LeftSideStreetName = dutchName;
                        }

                        if (dbRecord.RightSideStreetNameId == streetNameLocalId)
                        {
                            dbRecord.RightSideStreetName = dutchName;
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

        private async Task<StreetNameCacheItem> TryGetFromStreetNameCache(
            int? streetNameId,
            CancellationToken token)
        {
            return streetNameId.HasValue ? await _streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) : null;
        }
    }
}
