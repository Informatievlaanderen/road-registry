namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using Model;
    using RoadSegmentAccessRestriction = Model.RoadSegmentAccessRestriction;
    using RoadSegmentCategory = Model.RoadSegmentCategory;
    using RoadSegmentGeometryDrawMethod = Model.RoadSegmentGeometryDrawMethod;
    using RoadSegmentMorphology = Model.RoadSegmentMorphology;
    using RoadSegmentStatus = Model.RoadSegmentStatus;

    public class RoadSegmentRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly Encoding _encoding;

        public RoadSegmentRecordProjection(WellKnownBinaryReader wkbReader,
            Encoding encoding)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            var geometry = GeometryTranslator.Translate(@event.Geometry2);
            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var statusTranslation = RoadSegmentStatus.Parse(@event.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(@event.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(@event.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(@event.AccessRestriction).Translation;
            return context.AddAsync(
                new RoadSegmentRecord
                {
                    Id = @event.Id,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    Envelope = BoundingBox2D.From(polyLineMShapeContent.Shape.EnvelopeInternal),
                    DbaseRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = @event.Id },
                        WS_UIDN = { Value = $"{@event.Id}_{@event.Version}" },
                        WS_GIDN = { Value = $"{@event.Id}_{@event.GeometryVersion}" },
                        B_WK_OIDN = { Value = @event.StartNodeId },
                        E_WK_OIDN =  {Value = @event.EndNodeId },
                        STATUS = { Value = statusTranslation.Identifier },
                        LBLSTATUS = { Value = statusTranslation.Name },
                        MORF = { Value = morphologyTranslation.Identifier },
                        LBLMORF = { Value = morphologyTranslation.Name },
                        WEGCAT = { Value = categoryTranslation.Identifier },
                        LBLWEGCAT = { Value = categoryTranslation.Name },
                        LSTRNMID = { Value = @event.LeftSide.StreetNameId },
                        LSTRNM = { Value = @event.LeftSide.StreetName },
                        RSTRNMID = { Value = @event.RightSide.StreetNameId },
                        RSTRNM = { Value = @event.RightSide.StreetName },
                        BEHEER = { Value = @event.MaintenanceAuthority.Code },
                        LBLBEHEER = { Value = @event.MaintenanceAuthority.Name },
                        METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                        LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                        OPNDATUM = { Value = @event.RecordingDate },
                        BEGINTIJD = { Value = @event.Origin.Since },
                        BEGINORG = { Value = @event.Origin.OrganizationId },
                        LBLBGNORG = { Value = @event.Origin.Organization },
                        TGBEP = { Value = accessRestrictionTranslation.Identifier },
                        LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                    }.ToBytes(_encoding)
                },
                token);
        }
    }
}
