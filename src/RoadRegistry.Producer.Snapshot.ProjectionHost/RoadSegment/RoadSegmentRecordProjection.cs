namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Projections;
    using Syndication.Schema;

    public class RoadSegmentRecordProjection : ConnectedProjection<RoadSegmentProducerSnapshotContext>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public RoadSegmentRecordProjection(IKafkaProducer kafkaProducer, IStreetNameCache streetNameCache)
        {
            _kafkaProducer = kafkaProducer;
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
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
                    new RoadSegmentRecord()
                    {
                        Id = envelope.Message.Id,
                        BeginOperator = envelope.Message.Origin.Operator,
                        BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                        BeginOrganizationName = envelope.Message.Origin.Organization,
                        BeginTime = envelope.Message.Origin.Since,
                        BeginApplication = envelope.Message.Origin.Application,

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
                        LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode,
                        LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                        LeftSideStreetName = leftSideStreetNameRecord?.DutchNameWithHomonymAddition ??
                                             envelope.Message.LeftSide.StreetName,
                        RightSideMunicipalityId = null,
                        RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
                        RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                        RightSideStreetName = rightSideStreetNameRecord?.DutchNameWithHomonymAddition ??
                                              envelope.Message.RightSide.StreetName,

                        RoadSegmentVersion = envelope.Message.Version,

                        BeginRoadNodeId = envelope.Message.StartNodeId,
                        EndRoadNodeId = envelope.Message.EndNodeId,
                        StreetNameCachePosition = streetNameCachePosition,
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

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await RemoveRoadSegment(roadSegmentRemoved, context, envelope, token);
                            break;
                    }
            });
        }

        private async Task AddRoadSegment(IStreetNameCache streetNameCache,
            RoadSegmentProducerSnapshotContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadSegmentAdded roadSegmentAdded,
            CancellationToken token)
        {
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
                BeginOperator = envelope.Message.Operator,
                BeginOrganizationId = envelope.Message.OrganizationId,
                BeginOrganizationName = envelope.Message.Organization,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                BeginApplication = null,

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
                LeftSideStreetName = leftSideStreetNameRecord?.DutchName,

                RightSideMunicipalityId = null,
                RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                RightSideStreetName = rightSideStreetNameRecord?.DutchName,

                RoadSegmentVersion = roadSegmentAdded.Version,

                BeginRoadNodeId = roadSegmentAdded.StartNodeId,
                EndRoadNodeId = roadSegmentAdded.EndNodeId,
                StreetNameCachePosition = streetNameCachePosition
            };

            roadSegmentRecord.LastChangedTimestamp = roadSegmentRecord.LastChangedTimestamp;
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

            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentModified.Id).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadNodeRecord with id {roadSegmentModified.Id} is not found!");
            }

            roadSegmentRecord.BeginOperator = envelope.Message.Operator;
            roadSegmentRecord.BeginOrganizationId = envelope.Message.OrganizationId;
            roadSegmentRecord.BeginOrganizationName = envelope.Message.Organization;
            roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadSegmentRecord.BeginApplication = null;

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

            roadSegmentRecord.RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadSegmentRecord.TransactionId = transactionId == TransactionId.Unknown ? default(int?) : transactionId.ToInt32();

            roadSegmentRecord.LeftSideMunicipalityId = null;
            roadSegmentRecord.LeftSideMunicipalityNisCode = leftSideStreetNameRecord?.NisCode;
            roadSegmentRecord.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
            roadSegmentRecord.LeftSideStreetName = leftSideStreetNameRecord?.DutchName;

            roadSegmentRecord.RightSideMunicipalityId = null;
            roadSegmentRecord.RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode;
            roadSegmentRecord.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
            roadSegmentRecord.RightSideStreetName = rightSideStreetNameRecord?.DutchName;

            roadSegmentRecord.RoadSegmentVersion = roadSegmentModified.Version;

            roadSegmentRecord.BeginRoadNodeId = roadSegmentModified.StartNodeId;
            roadSegmentRecord.EndRoadNodeId = roadSegmentModified.EndNodeId;
            roadSegmentRecord.StreetNameCachePosition = streetNameCachePosition;
            roadSegmentRecord.LastChangedTimestamp = roadSegmentRecord.LastChangedTimestamp;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task RemoveRoadSegment(RoadSegmentRemoved roadSegmentRemoved, RoadSegmentProducerSnapshotContext context, Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken token)
        {
            var roadSegmentRecord = await context.RoadSegments.FindAsync(roadSegmentRemoved.Id).ConfigureAwait(false);
            if (roadSegmentRecord == null)
            {
                throw new InvalidOperationException($"RoadNodeRecord with id {roadSegmentRemoved.Id} is not found!");
            }

            roadSegmentRecord.BeginOperator = envelope.Message.Operator;
            roadSegmentRecord.BeginOrganizationId = envelope.Message.OrganizationId;
            roadSegmentRecord.BeginOrganizationName = envelope.Message.Organization;
            roadSegmentRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadSegmentRecord.BeginApplication = null;
            roadSegmentRecord.IsRemoved = true;
            roadSegmentRecord.LastChangedTimestamp = roadSegmentRecord.LastChangedTimestamp;

            await Produce(roadSegmentRecord.Id, roadSegmentRecord.ToContract(), token);
        }

        private async Task Produce(int roadNodeId, RoadSegmentSnapshot snapshot, CancellationToken cancellationToken)
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

        private static async Task<StreetNameRecord> TryGetFromCache(
            IStreetNameCache streetNameCache,
            int? streetNameId,
            CancellationToken token)
        {
            return streetNameId.HasValue ? await streetNameCache.GetAsync(streetNameId.Value, token).ConfigureAwait(false) : null;
        }
    }
}
