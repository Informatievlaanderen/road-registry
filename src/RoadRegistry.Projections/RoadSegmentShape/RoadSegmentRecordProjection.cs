namespace RoadRegistry.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Events;
    using Shaperon;
    using Wkx;

    public class RoadSegmentRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly IOrganizationRetriever _organizationRetriever;
        private readonly RoadSegmentStatusTranslator _segmentStatusTranslator;
        private readonly RoadSegmentMorphologyTranslator _segmentMorphologyTranslator;
        private readonly RoadSegmentCategoryTranslator _segmentCategoryTranslator;
        private readonly RoadSegmentGeometryDrawMethodTranslator _geometryDrawMethodTranslator;
        private readonly RoadSegmentAccessRestrictionTranslator _accessRestrictionTranslator;

        public RoadSegmentRecordProjection(IOrganizationRetriever organizationRetriever)
        {
            _organizationRetriever = organizationRetriever ?? throw new ArgumentNullException(nameof(organizationRetriever));
            _segmentStatusTranslator = new RoadSegmentStatusTranslator();
            _segmentMorphologyTranslator = new RoadSegmentMorphologyTranslator();
            _segmentCategoryTranslator = new RoadSegmentCategoryTranslator();
            _geometryDrawMethodTranslator = new RoadSegmentGeometryDrawMethodTranslator();
            _accessRestrictionTranslator = new RoadSegmentAccessRestrictionTranslator();

            When<ImportedRoadSegment>(HandleImportedRoadSegment);
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            var geometry = From
                .WellKnownBinary(@event.Geometry.WellKnownBinary)
                .To<MultiLineString>();

            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var organization = _organizationRetriever.Get(@event.MaintainerId);
            return context.AddAsync(
                new RoadSegmentRecord
                {
                    Id = @event.Id,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = @event.Id },
                        WS_UIDN = { Value = $"{@event.Id}_{@event.Version}" },
                        WS_GIDN = { Value = $"{@event.Id}_{@event.Geometry.Version}" },
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
                        BEHEER = { Value = @event.MaintainerId },
                        LBLBEHEER = { Value = organization.Name },
                        METHODE = { Value = _geometryDrawMethodTranslator.TranslateToIdentifier(@event.GeometryDrawMethod) },
                        LBLMETHOD = { Value = _geometryDrawMethodTranslator.TranslateToDutchName(@event.GeometryDrawMethod) },
                        OPNDATUM = { Value = @event.RecordingDate },
                        BEGINTIJD = { Value = @event.Origin.Since },
                        BEGINORG = { Value = @event.Origin.OrganizationId },
                        LBLBGNORG = { Value = @event.Origin.Organization },
                        TGBEP = { Value = _accessRestrictionTranslator.TranslateToIdentifier(@event.AccessRestriction) },
                        LBLTGBEP = { Value = _accessRestrictionTranslator.TranslateToDutchName(@event.AccessRestriction) },
                    }.ToBytes()
                },
                token);
        }
    }
}
