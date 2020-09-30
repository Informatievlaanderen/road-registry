namespace RoadRegistry.Wms.Projections
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Schema;
    using Syndication.Schema;

    public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
    {
        public RoadSegmentRecordProjection(IStreetNameCache streetNameCache)
        {
            When<Envelope<SynchronizeWithStreetNameCache>>(async (context, envelope, token) =>
            {
                var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);

                var outdatedRoadSegments = await context.RoadSegments
                    .Where(record => record.StreetNameCachePosition < streetNameCachePosition)
                    .OrderBy(record => record.StreetNameCachePosition)
                    .Take(envelope.Message.BatchSize)
                    .ToListAsync();

                var outdatedStreetNameIds = outdatedRoadSegments
                    .Select(record => record.LeftSideStreetNameId)
                    .Union(outdatedRoadSegments.Select(record => record.RightSideStreetNameId))
                    .Where(i => i.HasValue)
                    .Select(i => i.Value);

                var streetNamesById = await streetNameCache.GetStreetNamesByIdAsync(outdatedStreetNameIds, token);

                foreach (var roadSegment in outdatedRoadSegments)
                {
                    if (roadSegment.LeftSideStreetNameId.HasValue &&
                        streetNamesById.ContainsKey(roadSegment.LeftSideStreetNameId.Value))
                        roadSegment.LeftSideStreetName = streetNamesById[roadSegment.LeftSideStreetNameId.Value];

                    if(roadSegment.RightSideStreetNameId.HasValue &&
                       streetNamesById.ContainsKey(roadSegment.RightSideStreetNameId.Value))
                        roadSegment.RightSideStreetName = streetNamesById[roadSegment.RightSideStreetNameId.Value];

                    roadSegment.StreetNameCachePosition = streetNameCachePosition;
                }
            });

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

                await context.RoadSegments.AddAsync(new RoadSegmentRecord
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

                    Geometry2D = WmsGeometryTranslator.Translate2D(envelope.Message.Geometry),
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
                }, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                var transactionId = new TransactionId(envelope.Message.TransactionId);

                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAdded m:

                            var method =
                                RoadSegmentGeometryDrawMethod.Parse(m.GeometryDrawMethod);

                            var accessRestriction =
                                RoadSegmentAccessRestriction.Parse(m.AccessRestriction);

                            var status = RoadSegmentStatus.Parse(m.Status);

                            var morphology = RoadSegmentMorphology.Parse(m.Morphology);

                            var category = RoadSegmentCategory.Parse(m.Category);

                            var streetNameCachePosition = await streetNameCache.GetMaxPositionAsync(token);
                            var leftSideStreetNameRecord = await TryGetFromCache(streetNameCache, m.LeftSide.StreetNameId, token);
                            var rightSideStreetNameRecord = await TryGetFromCache(streetNameCache, m.RightSide.StreetNameId, token);

                            await context.RoadSegments.AddAsync(new RoadSegmentRecord
                            {
                                Id = m.Id,
                                BeginOperator = envelope.Message.Operator,
                                BeginOrganizationId = envelope.Message.OrganizationId,
                                BeginOrganizationName = envelope.Message.Organization,
                                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                                BeginApplication = null,

                                MaintainerId = m.MaintenanceAuthority.Code,
                                MaintainerName = m.MaintenanceAuthority.Name,

                                MethodId = method.Translation.Identifier,
                                MethodDutchName = method.Translation.Name,

                                CategoryId = category.Translation.Identifier,
                                CategoryDutchName = category.Translation.Name,

                                Geometry2D = WmsGeometryTranslator.Translate2D(m.Geometry),
                                GeometryVersion = m.GeometryVersion,

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
                                LeftSideStreetNameId = m.LeftSide.StreetNameId,
                                LeftSideStreetName = leftSideStreetNameRecord?.DutchName,

                                RightSideMunicipalityId = null,
                                RightSideMunicipalityNisCode = rightSideStreetNameRecord?.NisCode,
                                RightSideStreetNameId = m.RightSide.StreetNameId,
                                RightSideStreetName = rightSideStreetNameRecord?.DutchName,

                                RoadSegmentVersion = m.Version,

                                BeginRoadNodeId = m.StartNodeId,
                                EndRoadNodeId = m.EndNodeId,
                                StreetNameCachePosition = streetNameCachePosition,
                            }, token);
                            break;
                    }
                }
            });
        }

        private static async Task<StreetNameRecord> TryGetFromCache(
            IStreetNameCache streetNameCache,
            int? streetNameId,
            CancellationToken token)
        {
            return streetNameId.HasValue ?
                await streetNameCache.GetAsync(streetNameId.Value, token) :
                null;
        }
    }
}
