namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using Integration.Projections.Version;
using Schema.RoadSegments;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public class RoadSegmentsStreetNamesChangedScenarioBuilder : RoadSegmentScenarioBuilderBase
{
    private readonly List<Action<RoadSegmentAdded, RoadSegmentsStreetNamesChanged>> _setups = [];

    public RoadSegmentsStreetNamesChangedScenarioBuilder(Fixture fixture)
        : base(fixture)
    {
    }

    public RoadSegmentsStreetNamesChangedScenarioBuilder Setup(Action<RoadSegmentAdded, RoadSegmentsStreetNamesChanged> setup)
    {
        _setups.Add(setup);
        return this;
    }

    public Task Expect(Action<(
        RoadNetworkChangesAccepted AcceptedRoadSegmentAdded,
        RoadSegmentAdded RoadSegmentAdded,
        RoadSegmentsStreetNamesChanged RoadSegmentsStreetNamesChanged
        ), RoadSegmentVersion> customize = null)
    {
        Fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = Fixture.CreateWith<RoadSegmentAdded>(x =>
        {
            x.Lanes = x.Lanes.Select((attr, index) =>
            {
                attr.AttributeId = index + 1;
                return attr;
            }).ToArray();
            x.Surfaces = x.Surfaces.Select((attr, index) =>
            {
                attr.AttributeId = index + 1;
                return attr;
            }).ToArray();
            x.Widths = x.Widths.Select((attr, index) =>
            {
                attr.AttributeId = index + 1;
                return attr;
            }).ToArray();
        });
        var acceptedRoadSegmentAdded = Fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentsStreetNamesChanged = Fixture.Create<RoadSegmentsStreetNamesChanged>();
        var roadSegmentStreetNamesChanged = roadSegmentsStreetNamesChanged.RoadSegments.Single();

        foreach (var setup in _setups)
        {
            setup.Invoke(roadSegmentAdded, roadSegmentsStreetNamesChanged);
        }

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(roadSegmentsStreetNamesChanged.RoadSegments, _ =>
            {
                var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

                var position = 1;

                var roadSegmentVersion = new RoadSegmentVersion
                {
                    Position = position,
                    Id = roadSegmentAdded.Id,
                    StartNodeId = roadSegmentAdded.StartNodeId,
                    EndNodeId = roadSegmentAdded.EndNodeId,
                    Geometry = geometry,

                    Version = roadSegmentStreetNamesChanged.Version,
                    GeometryVersion = roadSegmentAdded.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = roadSegmentStreetNamesChanged.LeftSideStreetNameId,
                    RightSideStreetNameId = roadSegmentStreetNamesChanged.RightSideStreetNameId,
                    MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(roadSegmentsStreetNamesChanged.When),

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

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

                customize?.Invoke((acceptedRoadSegmentAdded, roadSegmentAdded, roadSegmentsStreetNamesChanged), roadSegmentVersion);

                return roadSegmentVersion;
            }));

        return new RoadSegmentVersionProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, roadSegmentsStreetNamesChanged)
            .Expect(expectedRecords);
    }
}
