namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Shared;

    public class RoadSegmentRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly RoadSegmentStatusTranslator _segmentStatusTranslator;
        private readonly RoadSegmentMorphologyTranslator _segmentMorphologyTranslator;
        private readonly RoadSegmentCategoryTranslator _segmentCategoryTranslator;
        private readonly RoadSegmentGeometryDrawMethodTranslator _geometryDrawMethodTranslator;
        private readonly RoadSegmentAccessRestrictionTranslator _accessRestrictionTranslator;
        private readonly Encoding _encoding;

        public RoadSegmentRecordProjection(WellKnownBinaryReader wkbReader,
            RoadSegmentStatusTranslator segmentStatusTranslator,
            RoadSegmentMorphologyTranslator segmentMorphologyTranslator,
            RoadSegmentCategoryTranslator segmentCategoryTranslator,
            RoadSegmentGeometryDrawMethodTranslator geometryDrawMethodTranslator,
            RoadSegmentAccessRestrictionTranslator accessRestrictionTranslator,
            Encoding encoding)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _segmentStatusTranslator = segmentStatusTranslator ?? throw new ArgumentNullException(nameof(segmentStatusTranslator));
            _segmentMorphologyTranslator = segmentMorphologyTranslator ?? throw new ArgumentNullException(nameof(segmentMorphologyTranslator));
            _segmentCategoryTranslator = segmentCategoryTranslator ?? throw new ArgumentNullException(nameof(segmentCategoryTranslator));
            _geometryDrawMethodTranslator = geometryDrawMethodTranslator ?? throw new ArgumentNullException(nameof(geometryDrawMethodTranslator));
            _accessRestrictionTranslator = accessRestrictionTranslator ?? throw new ArgumentNullException(nameof(accessRestrictionTranslator));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            var geometry = _wkbReader.TryReadAs(@event.Geometry, out LineString line)
                ? new MultiLineString(new ILineString[] { line })
                : _wkbReader.ReadAs<MultiLineString>(@event.Geometry);

            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
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
                        STATUS = { Value = _segmentStatusTranslator.TranslateToIdentifier(@event.Status) },
                        LBLSTATUS = { Value = _segmentStatusTranslator.TranslateToDutchName(@event.Status) },
                        MORF = { Value = _segmentMorphologyTranslator.TranslateToIdentifier(@event.Morphology) },
                        LBLMORF = { Value = _segmentMorphologyTranslator.TranslateToDutchName(@event.Morphology) },
                        WEGCAT = { Value = _segmentCategoryTranslator.TranslateToIdentifier(@event.Category) },
                        LBLWEGCAT = { Value = _segmentCategoryTranslator.TranslateToDutchName(@event.Category) },
                        LSTRNMID = { Value = @event.LeftSide.StreetNameId },
                        LSTRNM = { Value = @event.LeftSide.StreetName },
                        RSTRNMID = { Value = @event.RightSide.StreetNameId },
                        RSTRNM = { Value = @event.RightSide.StreetName },
                        BEHEER = { Value = @event.Maintainer.Code },
                        LBLBEHEER = { Value = @event.Maintainer.Name },
                        METHODE = { Value = _geometryDrawMethodTranslator.TranslateToIdentifier(@event.GeometryDrawMethod) },
                        LBLMETHOD = { Value = _geometryDrawMethodTranslator.TranslateToDutchName(@event.GeometryDrawMethod) },
                        OPNDATUM = { Value = @event.RecordingDate },
                        BEGINTIJD = { Value = @event.Origin.Since },
                        BEGINORG = { Value = @event.Origin.OrganizationId },
                        LBLBGNORG = { Value = @event.Origin.Organization },
                        TGBEP = { Value = _accessRestrictionTranslator.TranslateToIdentifier(@event.AccessRestriction) },
                        LBLTGBEP = { Value = _accessRestrictionTranslator.TranslateToDutchName(@event.AccessRestriction) },
                    }.ToBytes(_encoding)
                },
                token);
        }
    }
}
