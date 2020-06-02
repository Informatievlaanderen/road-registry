namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Text;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
    {
        public RoadSegmentRecordProjection(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var roadSegmentGeometryDrawMethod =
                    RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);

                var accessRestriction =
                    RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);

                var roadSegmentStatus = RoadSegmentStatus.Parse(envelope.Message.Status);

                var roadSegmentMorphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);

                var roadSegmentCategory = RoadSegmentCategory.Parse(envelope.Message.Category);

                await context.RoadSegments.AddAsync(new RoadSegmentRecord
                {
                    Id = envelope.Message.Id,
                    BeginOperator = OrganizationId.Unknown,
                    BeginOrganization = envelope.Message.Origin.OrganizationId,
                    BeginTime = envelope.Message.Origin.Since,
                    BeginApplication = "-8",

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
                    TransactionId = 0,

                    LeftSideMunicipality = 0,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    LeftSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.LeftSide.StreetName)
                        ? null
                        : envelope.Message.LeftSide.StreetName,

                    RightSideMunicipality = 0,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    RightSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.RightSide.StreetName)
                        ? null
                        : envelope.Message.RightSide.StreetName,

                    RoadSegmentVersion = envelope.Message.Version,

                    BeginRoadNodeId = envelope.Message.StartNodeId,
                    EndRoadNodeId = envelope.Message.EndNodeId
                }, token);
            });
        }
    }
}
