namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using Integration.Projections.Version;
using RoadSegment.ValueObjects;
using Schema.RoadSegments;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public class ModifyRoadSegmentGeometryScenarioBuilder: RoadSegmentScenarioBuilderBase
{
    private readonly List<Action<RoadSegmentAdded, RoadSegmentGeometryModified>> _setups = [];

    public ModifyRoadSegmentGeometryScenarioBuilder(Fixture fixture)
        : base(fixture)
    {
    }

    public ModifyRoadSegmentGeometryScenarioBuilder Setup(Action<RoadSegmentAdded, RoadSegmentGeometryModified> setup)
    {
        _setups.Add(setup);
        return this;
    }

    public Task Expect(Action<(
        RoadNetworkChangesAccepted AcceptedRoadSegmentAdded,
        RoadNetworkChangesAccepted AcceptedRoadSegmentGeometryModified,
        RoadSegmentAdded RoadSegmentAdded,
        RoadSegmentGeometryModified RoadSegmentGeometryModified
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

        var roadSegmentGeometryModified = Fixture.CreateWith<RoadSegmentGeometryModified>(x =>
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
        var acceptedRoadSegmentGeometryModified = Fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentGeometryModified);

        foreach (var setup in _setups)
        {
            setup.Invoke(roadSegmentAdded, roadSegmentGeometryModified);
        }

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, _ =>
            {
                var geometry = GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
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
                    Id = roadSegmentGeometryModified.Id,
                    StartNodeId = roadSegmentAdded.StartNodeId,
                    EndNodeId = roadSegmentAdded.EndNodeId,
                    Geometry = geometry,

                    Version = roadSegmentGeometryModified.Version,
                    GeometryVersion = roadSegmentGeometryModified.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                    RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                    MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    Lanes = roadSegmentGeometryModified.Lanes
                        .Select(x => new RoadSegmentLaneAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentGeometryModified.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Count = x.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                            OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                        })
                        .ToList(),
                    Surfaces = roadSegmentGeometryModified.Surfaces
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

                            OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                        })
                        .ToList(),
                    Widths = roadSegmentGeometryModified.Widths
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

                            OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

                customize?.Invoke((acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified, roadSegmentAdded, roadSegmentGeometryModified), roadSegmentVersion);

                return roadSegmentVersion;
            }));

        return new RoadSegmentVersionProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .Expect(expectedRecords);
    }
}
