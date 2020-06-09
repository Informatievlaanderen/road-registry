namespace RoadRegistry.Wms.Projections
{
    using System;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
    {
        public RoadSegmentRecordProjection()
        {
            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var roadSegmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var roadSegmentStatus = RoadSegmentStatus.Parse(envelope.Message.Status);
                var roadSegmentMorphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var roadSegmentCategory = RoadSegmentCategory.Parse(envelope.Message.Category);

                await context.RoadSegments.AddAsync(new RoadSegmentRecord
                {
                    Id = envelope.Message.Id,
                    BeginOperator = envelope.Message.Origin.Operator,
                    BeginOrganization = envelope.Message.Origin.OrganizationId,
                    BeginTime = envelope.Message.Origin.Since,
                    BeginApplication = envelope.Message.Origin.Application,

                    Maintainer = envelope.Message.MaintenanceAuthority.Code,
                    MaintainerLabel = envelope.Message.MaintenanceAuthority.Name,

                    Method = roadSegmentGeometryDrawMethod.Translation.Identifier,
                    MethodLabel = roadSegmentGeometryDrawMethod.Translation.Name,

                    Category = roadSegmentCategory.Translation.Identifier,
                    CategoryLabel = roadSegmentCategory.Translation.Name,

                    Geometry = WmsGeometryTranslator.Translate3D(envelope.Message.Geometry),
                    Geometry2D = WmsGeometryTranslator.Translate2D(envelope.Message.Geometry),
                    GeometryVersion = envelope.Message.GeometryVersion,

                    Morphology = roadSegmentMorphology.Translation.Identifier,
                    MorphologyLabel = roadSegmentMorphology.Translation.Name,

                    Status = roadSegmentStatus.Translation.Identifier,
                    StatusLabel = roadSegmentStatus.Translation.Name,

                    AccessRestriction = accessRestriction.Translation.Identifier,
                    AccessRestrictionLabel = accessRestriction.Translation.Name,

                    SourceId = null,
                    SourceIdSource = null,

                    OrganizationLabel = envelope.Message.Origin.Organization,
                    RecordingDate = envelope.Message.RecordingDate,
                    TransactionId = envelope.Message.Origin.TransactionId,

                    LeftSideMunicipality = null,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    LeftSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.LeftSide.StreetName)
                        ? null
                        : envelope.Message.LeftSide.StreetName,

                    RightSideMunicipality = null,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    RightSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.RightSide.StreetName)
                        ? null
                        : envelope.Message.RightSide.StreetName,

                    RoadSegmentVersion = envelope.Message.Version,

                    BeginRoadNodeId = envelope.Message.StartNodeId,
                    EndRoadNodeId = envelope.Message.EndNodeId
                }, token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAdded m:

                            var roadSegmentGeometryDrawMethod =
                                RoadSegmentGeometryDrawMethod.Parse(m.GeometryDrawMethod);

                            var accessRestriction =
                                RoadSegmentAccessRestriction.Parse(m.AccessRestriction);

                            var roadSegmentStatus = RoadSegmentStatus.Parse(m.Status);

                            var roadSegmentMorphology = RoadSegmentMorphology.Parse(m.Morphology);

                            var roadSegmentCategory = RoadSegmentCategory.Parse(m.Category);

                            await context.RoadSegments.AddAsync(new RoadSegmentRecord
                            {
                                Id = m.Id,
                                BeginOperator = envelope.Message.Operator,
                                BeginOrganization = envelope.Message.Organization,
                                BeginTime = DateTime.Parse(envelope.Message.When),
                                BeginApplication = null,

                                Maintainer = m.MaintenanceAuthority.Code,
                                MaintainerLabel = m.MaintenanceAuthority.Name,

                                Method = roadSegmentGeometryDrawMethod.Translation.Identifier,
                                MethodLabel = roadSegmentGeometryDrawMethod.Translation.Name,

                                Category = roadSegmentCategory.Translation.Identifier,
                                CategoryLabel = roadSegmentCategory.Translation.Name,

                                Geometry = WmsGeometryTranslator.Translate3D(m.Geometry),
                                Geometry2D = WmsGeometryTranslator.Translate2D(m.Geometry),
                                GeometryVersion = m.GeometryVersion,

                                Morphology = roadSegmentMorphology.Translation.Identifier,
                                MorphologyLabel = roadSegmentMorphology.Translation.Name,

                                Status = roadSegmentStatus.Translation.Identifier,
                                StatusLabel = roadSegmentStatus.Translation.Name,

                                AccessRestriction = accessRestriction.Translation.Identifier,
                                AccessRestrictionLabel = accessRestriction.Translation.Name,

                                SourceId = null,
                                SourceIdSource = null,

                                OrganizationLabel = envelope.Message.Organization,
                                RecordingDate = DateTime.Parse(envelope.Message.When),
                                TransactionId = null,

                                LeftSideMunicipality = null,
                                LeftSideStreetNameId = m.LeftSide.StreetNameId,
                                LeftSideStreetNameLabel = null,

                                RightSideMunicipality = null,
                                RightSideStreetNameId = m.RightSide.StreetNameId,
                                RightSideStreetNameLabel = null,

                                RoadSegmentVersion = m.Version,

                                BeginRoadNodeId = m.StartNodeId,
                                EndRoadNodeId = m.EndNodeId
                            }, token);
                            break;
                    }
                }
            });
        }
    }
}
