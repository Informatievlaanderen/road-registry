namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Integration.Schema.RoadSegments.Version;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_lanes()
    {
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(1)
            .Select((importedRoadSegment, eventIndex) =>
            {
                importedRoadSegment.Lanes = [];

                var expected = BuildRoadSegmentRecord(eventIndex, importedRoadSegment);
                expected.Lanes = [];

                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_lanes_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Lanes = roadSegmentModified.Lanes
                    .Select(laneAttribute =>
                    {
                        var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                        roadSegmentLaneAttributes.AttributeId = laneAttribute.AttributeId;
                        return roadSegmentLaneAttributes;
                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_lanes_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Lanes = [];
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = Array.Empty<RoadSegmentLaneAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Lanes
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,

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
    public Task When_modifying_road_segments_with_some_added_lanes()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, _) =>
            {
                roadSegmentAdded.Lanes = roadSegmentAdded.Lanes.Take(roadSegmentAdded.Lanes.Length - 1).ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = context.RoadSegmentModified.Lanes
                    .Select(laneAttribute =>
                    {
                        var isAdded = context.RoadSegmentAdded.Lanes.All(x => x.AttributeId != laneAttribute.AttributeId);

                        return new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,

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
    public Task When_modifying_road_segments_with_some_modified_lanes()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, roadSegmentModified) =>
            {
                roadSegmentModified.Lanes = roadSegmentAdded.Lanes
                    .Select((laneAttribute, i) =>
                    {
                        if (i % 2 != 0)
                        {
                            return laneAttribute;
                        }

                        var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                        roadSegmentLaneAttributes.AttributeId = laneAttribute.AttributeId;
                        return roadSegmentLaneAttributes;

                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_lanes()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Lanes = roadSegmentModified.Lanes
                    .Skip(1)
                    .ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = Array.Empty<RoadSegmentLaneAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Lanes.Take(1)
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Lanes
                        .Select(laneAttribute =>
                            new RoadSegmentLaneAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = laneAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                                Count = laneAttribute.Count,
                                DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                                DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                                FromPosition = (double)laneAttribute.FromPosition,
                                ToPosition = (double)laneAttribute.ToPosition,
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
    public Task When_modifying_road_segment_geometry_with_new_lanes_only()
    {
        return new ModifyRoadSegmentGeometryScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Lanes.Length; i++)
                {
                    roadSegmentModified.Lanes[i].AttributeId = i + 1 + roadSegmentModified.Lanes.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = Array.Empty<RoadSegmentLaneAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Lanes
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentGeometryModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentGeometryModified.When)
                        }))
                    .Concat(context.RoadSegmentGeometryModified.Lanes
                        .Select(laneAttribute =>
                            new RoadSegmentLaneAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = laneAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                                Count = laneAttribute.Count,
                                DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                                DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                                FromPosition = (double)laneAttribute.FromPosition,
                                ToPosition = (double)laneAttribute.ToPosition,
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
    public Task When_modifying_road_segments_with_new_lanes_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Lanes.Length; i++)
                {
                    roadSegmentModified.Lanes[i].AttributeId = i + 1 + roadSegmentModified.Lanes.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = Array.Empty<RoadSegmentLaneAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Lanes
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Lanes
                        .Select(lane =>
                            new RoadSegmentLaneAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = lane.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                                Count = lane.Count,
                                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                                FromPosition = (double)lane.FromPosition,
                                ToPosition = (double)lane.ToPosition,
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
    public Task When_modifying_road_segment_attributes_with_new_lanes_only()
    {
        return new ModifyRoadSegmentAttributesScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Lanes.Length; i++)
                {
                    roadSegmentModified.Lanes[i].AttributeId = i + 1 + roadSegmentModified.Lanes.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Lanes = Array.Empty<RoadSegmentLaneAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Lanes
                        .Select(laneAttribute => new RoadSegmentLaneAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = laneAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                            Count = laneAttribute.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation.Name,
                            FromPosition = (double)laneAttribute.FromPosition,
                            ToPosition = (double)laneAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentAttributesModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAttributesModified.When)
                        }))
                    .Concat(context.RoadSegmentAttributesModified.Lanes
                        .Select(lane =>
                            new RoadSegmentLaneAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = lane.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                                Count = lane.Count,
                                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                                FromPosition = (double)lane.FromPosition,
                                ToPosition = (double)lane.ToPosition,
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
