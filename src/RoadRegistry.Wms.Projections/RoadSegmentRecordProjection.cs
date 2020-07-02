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
                var method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var status = RoadSegmentStatus.Parse(envelope.Message.Status);
                var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var category = RoadSegmentCategory.Parse(envelope.Message.Category);
                var transactionId = new TransactionId(envelope.Message.Origin.TransactionId);

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
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    LeftSideStreetName = string.IsNullOrWhiteSpace(envelope.Message.LeftSide.StreetName)
                        ? null
                        : envelope.Message.LeftSide.StreetName,

                    RightSideMunicipalityId = null,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    RightSideStreetName = string.IsNullOrWhiteSpace(envelope.Message.RightSide.StreetName)
                        ? null
                        : envelope.Message.RightSide.StreetName,

                    RoadSegmentVersion = envelope.Message.Version,

                    BeginRoadNodeId = envelope.Message.StartNodeId,
                    EndRoadNodeId = envelope.Message.EndNodeId
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
                                LeftSideStreetNameId = m.LeftSide.StreetNameId,
                                LeftSideStreetName = null,

                                RightSideMunicipalityId = null,
                                RightSideStreetNameId = m.RightSide.StreetNameId,
                                RightSideStreetName = null,

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
