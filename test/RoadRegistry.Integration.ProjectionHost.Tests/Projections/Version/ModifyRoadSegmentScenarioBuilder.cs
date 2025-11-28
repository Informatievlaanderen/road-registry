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

public class ModifyRoadSegmentScenarioBuilder: RoadSegmentScenarioBuilderBase
{
    private readonly List<Action<RoadSegmentAdded, RoadSegmentModified>> _setups = [];

    public ModifyRoadSegmentScenarioBuilder(Fixture fixture)
        : base(fixture)
    {
    }

    public ModifyRoadSegmentScenarioBuilder Setup(Action<RoadSegmentAdded, RoadSegmentModified> setup)
    {
        _setups.Add(setup);
        return this;
    }

    public Task Expect(Action<(
        RoadNetworkChangesAccepted AcceptedRoadSegmentAdded,
        RoadNetworkChangesAccepted AcceptedRoadSegmentModified,
        RoadSegmentAdded RoadSegmentAdded,
        RoadSegmentModified RoadSegmentModified
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

        var roadSegmentModified = Fixture.CreateWith<RoadSegmentModified>(x =>
        {
            x.Lanes = roadSegmentAdded.Lanes
                .Select(addedAttribute => Fixture.CreateWith<RoadSegmentLaneAttributes>(modifiedAttribute =>
                {
                    modifiedAttribute.AttributeId = addedAttribute.AttributeId;
                })).ToArray();
            x.Surfaces = roadSegmentAdded.Surfaces
                .Select(addedAttribute => Fixture.CreateWith<RoadSegmentSurfaceAttributes>(modifiedAttribute =>
                {
                    modifiedAttribute.AttributeId = addedAttribute.AttributeId;
                })).ToArray();
            x.Widths = roadSegmentAdded.Widths
                .Select(addedAttribute => Fixture.CreateWith<RoadSegmentWidthAttributes>(modifiedAttribute =>
                {
                    modifiedAttribute.AttributeId = addedAttribute.AttributeId;
                })).ToArray();
        });
        var acceptedRoadSegmentModified = Fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        foreach (var setup in _setups)
        {
            setup.Invoke(roadSegmentAdded, roadSegmentModified);
        }

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(acceptedRoadSegmentModified.Changes, _ =>
            {
                var geometry = GeometryTranslator.Translate(roadSegmentModified.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(roadSegmentModified.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentModified.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction).Translation;

                var position = 1;

                var roadSegmentVersion = new RoadSegmentVersion
                {
                    Position = position,
                    Id = roadSegmentModified.Id,
                    StartNodeId = roadSegmentModified.StartNodeId,
                    EndNodeId = roadSegmentModified.EndNodeId,
                    Geometry = geometry,

                    Version = roadSegmentModified.Version,
                    GeometryVersion = roadSegmentModified.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId,
                    RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId,
                    MaintainerId = roadSegmentModified.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentModified.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    Lanes = roadSegmentModified.Lanes
                        .Select(x => new RoadSegmentLaneAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentModified.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Count = x.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                            OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                        })
                        .ToList(),
                    Surfaces = roadSegmentModified.Surfaces
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

                            OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                        })
                        .ToList(),
                    Widths = roadSegmentModified.Widths
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

                            OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

                customize?.Invoke((acceptedRoadSegmentAdded, acceptedRoadSegmentModified, roadSegmentAdded, roadSegmentModified), roadSegmentVersion);

                return roadSegmentVersion;
            }));

        return new RoadSegmentVersionProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }
}
