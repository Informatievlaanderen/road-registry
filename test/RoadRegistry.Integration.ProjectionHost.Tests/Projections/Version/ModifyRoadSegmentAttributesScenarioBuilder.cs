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

public class ModifyRoadSegmentAttributesScenarioBuilder : RoadSegmentScenarioBuilderBase
{
    private readonly List<Action<RoadSegmentAdded, RoadSegmentAttributesModified>> _setups = [];

    public ModifyRoadSegmentAttributesScenarioBuilder(Fixture fixture)
        : base(fixture)
    {
    }

    public ModifyRoadSegmentAttributesScenarioBuilder Setup(Action<RoadSegmentAdded, RoadSegmentAttributesModified> setup)
    {
        _setups.Add(setup);
        return this;
    }

    public Task Expect(Action<(
        RoadNetworkChangesAccepted AcceptedRoadSegmentAdded,
        RoadNetworkChangesAccepted AcceptedRoadSegmentAttributesModified,
        RoadSegmentAdded RoadSegmentAdded,
        RoadSegmentAttributesModified RoadSegmentAttributesModified
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

        var roadSegmentAttributesModified = Fixture.CreateWith<RoadSegmentAttributesModified>(x =>
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
        var acceptedRoadSegmentAttributesModified = Fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAttributesModified);

        foreach (var setup in _setups)
        {
            setup.Invoke(roadSegmentAdded, roadSegmentAttributesModified);
        }

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(acceptedRoadSegmentAttributesModified.Changes, _ =>
            {
                var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction).Translation;

                var position = 1;

                var roadSegmentVersion = new RoadSegmentVersion
                {
                    Position = position,
                    Id = roadSegmentAttributesModified.Id,
                    StartNodeId = roadSegmentAdded.StartNodeId,
                    EndNodeId = roadSegmentAdded.EndNodeId,
                    Geometry = geometry,

                    Version = roadSegmentAttributesModified.Version,
                    GeometryVersion = roadSegmentAdded.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = roadSegmentAttributesModified.LeftSide.StreetNameId,
                    RightSideStreetNameId = roadSegmentAttributesModified.RightSide.StreetNameId,
                    MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    OrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAttributesModified.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    Lanes = roadSegmentAttributesModified.Lanes
                        .Select(x => new RoadSegmentLaneAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentAttributesModified.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Count = x.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                            OrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentAttributesModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When)
                        })
                        .ToList(),
                    Surfaces = roadSegmentAttributesModified.Surfaces
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

                            OrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentAttributesModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When)
                        })
                        .ToList(),
                    Widths = roadSegmentAttributesModified.Widths
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

                            OrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentAttributesModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When)
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

                customize?.Invoke((acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified, roadSegmentAdded, roadSegmentAttributesModified), roadSegmentVersion);

                return roadSegmentVersion;
            }));

        return new RoadSegmentVersionProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified)
            .Expect(expectedRecords);
    }
}
