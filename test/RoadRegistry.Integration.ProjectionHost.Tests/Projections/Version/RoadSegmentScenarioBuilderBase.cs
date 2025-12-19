namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using Schema.RoadSegments;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public class RoadSegmentScenarioBuilderBase
{
    protected readonly Fixture Fixture;

    public RoadSegmentScenarioBuilderBase(Fixture fixture)
    {
        Fixture = fixture;
    }

    protected object[] BuildInitialExpectedRoadSegmentRecords(RoadNetworkChangesAccepted message)
    {
        return message.Changes
            .Select(change => BuildRoadSegmentRecord(IntegrationContextScenarioExtensions.InitialPosition, message, change.RoadSegmentAdded))
            .Cast<object>()
            .ToArray();
    }

    private RoadSegmentVersion BuildRoadSegmentRecord(
        long position,
        RoadNetworkChangesAccepted acceptedRoadSegmentAdded,
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentVersion> changeRecord = null)
    {
        var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

        var record = new RoadSegmentVersion
        {
            Position = position,
            Id = roadSegmentAdded.Id,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            Geometry = geometry,

            Version = roadSegmentAdded.Version,
            GeometryVersion = roadSegmentAdded.GeometryVersion,
            StatusId = statusTranslation.Identifier,
            StatusLabel = statusTranslation.Name,
            MorphologyId = morphologyTranslation.Identifier,
            MorphologyLabel = morphologyTranslation.Name,
            CategoryId = categoryTranslation.Identifier,
            CategoryLabel = categoryTranslation.Name,
            LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
            RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
            MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
            MethodId = geometryDrawMethodTranslation.Identifier,
            MethodLabel = geometryDrawMethodTranslation.Name,
            AccessRestrictionId = accessRestrictionTranslation.Identifier,
            AccessRestrictionLabel = accessRestrictionTranslation.Name,

            OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
            OrganizationName = acceptedRoadSegmentAdded.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

            Lanes = roadSegmentAdded.Lanes
                .Select(x => new RoadSegmentLaneAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    Count = x.Count,
                    DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList(),
            Surfaces = roadSegmentAdded.Surfaces
                .Select(x => new RoadSegmentSurfaceAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    TypeId = RoadSegmentSurfaceType.Parse(x.Type).Translation.Identifier,
                    TypeLabel = RoadSegmentSurfaceType.Parse(x.Type).Translation.Name,

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList(),
            Widths = roadSegmentAdded.Widths
                .Select(x => new RoadSegmentWidthAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    Width = x.Width,
                    WidthLabel = new RoadSegmentWidth(x.Width).ToDutchString(),

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList()
        }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        changeRecord?.Invoke(record);

        return record;
    }
}
