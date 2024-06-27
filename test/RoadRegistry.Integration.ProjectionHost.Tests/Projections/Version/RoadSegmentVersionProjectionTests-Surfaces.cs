namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using RoadRegistry.Integration.Schema.RoadSegments.Version;
using Schema.RoadSegments;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_surfaces()
    {
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(1)
            .Select((importedRoadSegment, eventIndex) =>
            {
                importedRoadSegment.Surfaces = [];

                var geometry = GeometryTranslator.Translate(importedRoadSegment.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation;

                var expected = new RoadSegmentVersion
                {
                    Position = eventIndex,
                    Id = importedRoadSegment.Id,
                    StartNodeId = importedRoadSegment.StartNodeId,
                    EndNodeId = importedRoadSegment.EndNodeId,
                    Geometry = geometry,

                    Version = importedRoadSegment.Version,
                    GeometryVersion = importedRoadSegment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = importedRoadSegment.LeftSide.StreetNameId,
                    RightSideStreetNameId = importedRoadSegment.RightSide.StreetNameId,
                    MaintainerId = importedRoadSegment.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    OrganizationId = importedRoadSegment.Origin.OrganizationId,
                    OrganizationName = importedRoadSegment.Origin.Organization,
                    CreatedOnTimestamp = importedRoadSegment.RecordingDate,
                    VersionTimestamp = importedRoadSegment.Origin.Since,

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    Lanes = importedRoadSegment.Lanes
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = eventIndex,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = importedRoadSegment.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,

                            OrganizationId = laneAttribute.Origin.OrganizationId,
                            OrganizationName = laneAttribute.Origin.Organization,
                            CreatedOnTimestamp = new DateTimeOffset(laneAttribute.Origin.Since),
                            VersionTimestamp = new DateTimeOffset(laneAttribute.Origin.Since)
                        })
                        .ToList(),
                    Surfaces = [],
                    Widths = importedRoadSegment.Widths
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = eventIndex,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = importedRoadSegment.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),

                            OrganizationId = widthAttribute.Origin.OrganizationId,
                            OrganizationName = widthAttribute.Origin.Organization,
                            CreatedOnTimestamp = new DateTimeOffset(widthAttribute.Origin.Since),
                            VersionTimestamp = new DateTimeOffset(widthAttribute.Origin.Since)
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_surfaces_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Surfaces = roadSegmentModified.Surfaces
                    .Select(surfaceAttribute =>
                    {
                        var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                        roadSegmentSurfaceAttributes.AttributeId = surfaceAttribute.AttributeId;
                        return roadSegmentSurfaceAttributes;
                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_surfaces_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Surfaces = [];
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = Array.Empty<RoadSegmentSurfaceAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Surfaces
                        .Select(surfaceAttribute => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,

                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When),

                            IsRemoved = true
                        }))
                    .ToList();
            });
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_added_surfaces()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, _) =>
            {
                roadSegmentAdded.Surfaces = roadSegmentAdded.Surfaces.Take(roadSegmentAdded.Surfaces.Length - 1).ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = context.RoadSegmentModified.Surfaces
                    .Select(surfaceAttribute =>
                    {
                        var isAdded = context.RoadSegmentAdded.Surfaces.All(x => x.AttributeId != surfaceAttribute.AttributeId);

                        return new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,

                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            CreatedOnTimestamp = isAdded
                                ? LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                                : LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        };
                    })
                    .ToList();
            });
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_modified_surfaces()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, roadSegmentModified) =>
            {
                roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
                    .Select((surfaceAttribute, i) =>
                    {
                        if (i % 2 != 0)
                        {
                            return surfaceAttribute;
                        }

                        var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                        roadSegmentSurfaceAttributes.AttributeId = surfaceAttribute.AttributeId;
                        return roadSegmentSurfaceAttributes;

                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_surfaces()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Surfaces = roadSegmentModified.Surfaces
                    .Skip(1)
                    .ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = Array.Empty<RoadSegmentSurfaceAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Surfaces.Take(1)
                        .Select(surfaceAttribute => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Surfaces
                        .Select(surfaceAttribute =>
                            new RoadSegmentSurfaceAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = surfaceAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                                TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                                TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                                FromPosition = (double)surfaceAttribute.FromPosition,
                                ToPosition = (double)surfaceAttribute.ToPosition,
                                OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                                OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                                IsRemoved = false,
                                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                            }
                        ))
                    .ToList();
            });
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_surfaces_only()
    {
        return new ModifyRoadSegmentGeometryScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Surfaces.Length; i++)
                {
                    roadSegmentModified.Surfaces[i].AttributeId = i + 1 + roadSegmentModified.Surfaces.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = Array.Empty<RoadSegmentSurfaceAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Surfaces
                        .Select(surfaceAttribute => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentGeometryModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentGeometryModified.When)
                        }))
                    .Concat(context.RoadSegmentGeometryModified.Surfaces
                        .Select(surfaceAttribute =>
                            new RoadSegmentSurfaceAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = surfaceAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                                TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                                TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                                FromPosition = (double)surfaceAttribute.FromPosition,
                                ToPosition = (double)surfaceAttribute.ToPosition,
                                OrganizationId = context.AcceptedRoadSegmentGeometryModified.OrganizationId,
                                OrganizationName = context.AcceptedRoadSegmentGeometryModified.Organization,
                                IsRemoved = false,
                                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentGeometryModified.When),
                                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentGeometryModified.When)
                            }
                        ))
                    .ToList();
            });
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_surfaces_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Surfaces.Length; i++)
                {
                    roadSegmentModified.Surfaces[i].AttributeId = i + 1 + roadSegmentModified.Surfaces.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = Array.Empty<RoadSegmentSurfaceAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Surfaces
                        .Select(surfaceAttribute => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Surfaces
                        .Select(surfaceAttribute =>
                            new RoadSegmentSurfaceAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = surfaceAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                                TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                                TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                                FromPosition = (double)surfaceAttribute.FromPosition,
                                ToPosition = (double)surfaceAttribute.ToPosition,
                                OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                                OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                                IsRemoved = false,
                                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When),
                                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                            }
                        ))
                    .ToList();
            });
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_surfaces_only()
    {
        return new ModifyRoadSegmentAttributesScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Surfaces.Length; i++)
                {
                    roadSegmentModified.Surfaces[i].AttributeId = i + 1 + roadSegmentModified.Surfaces.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Surfaces = Array.Empty<RoadSegmentSurfaceAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Surfaces
                        .Select(surfaceAttribute => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = surfaceAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                            TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                            FromPosition = (double)surfaceAttribute.FromPosition,
                            ToPosition = (double)surfaceAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentAttributesModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAttributesModified.When)
                        }))
                    .Concat(context.RoadSegmentAttributesModified.Surfaces
                        .Select(surfaceAttribute =>
                            new RoadSegmentSurfaceAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = surfaceAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                                TypeId = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Identifier,
                                TypeLabel = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation.Name,
                                FromPosition = (double)surfaceAttribute.FromPosition,
                                ToPosition = (double)surfaceAttribute.ToPosition,
                                OrganizationId = context.AcceptedRoadSegmentAttributesModified.OrganizationId,
                                OrganizationName = context.AcceptedRoadSegmentAttributesModified.Organization,
                                IsRemoved = false,
                                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAttributesModified.When),
                                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAttributesModified.When)
                            }
                        ))
                    .ToList();
            });
    }
}
