namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Integration.Schema.RoadSegments.Version;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_widths()
    {
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(1)
            .Select((importedRoadSegment, eventIndex) =>
            {
                importedRoadSegment.Widths = [];

                var expected = BuildRoadSegmentRecord(eventIndex, importedRoadSegment);
                expected.Widths = [];

                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_widths_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Widths = roadSegmentModified.Widths
                    .Select(widthAttribute =>
                    {
                        var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                        roadSegmentWidthAttributes.AttributeId = widthAttribute.AttributeId;
                        return roadSegmentWidthAttributes;
                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_widths_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Widths = [];
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = Array.Empty<RoadSegmentWidthAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Widths
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),

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
    public Task When_modifying_road_segments_with_some_added_widths()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, _) =>
            {
                roadSegmentAdded.Widths = roadSegmentAdded.Widths.Take(roadSegmentAdded.Widths.Length - 1).ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = context.RoadSegmentModified.Widths
                    .Select(widthAttribute =>
                    {
                        var isAdded = context.RoadSegmentAdded.Widths.All(x => x.AttributeId != widthAttribute.AttributeId);

                        return new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = context.RoadSegmentModified.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),

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
    public Task When_modifying_road_segments_with_some_modified_widths()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((roadSegmentAdded, roadSegmentModified) =>
            {
                roadSegmentModified.Widths = roadSegmentAdded.Widths
                    .Select((widthAttribute, i) =>
                    {
                        if (i % 2 != 0)
                        {
                            return widthAttribute;
                        }

                        var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                        roadSegmentWidthAttributes.AttributeId = widthAttribute.AttributeId;
                        return roadSegmentWidthAttributes;

                    })
                    .ToArray();
            })
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_widths()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                roadSegmentModified.Widths = roadSegmentModified.Widths
                    .Skip(1)
                    .ToArray();
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = Array.Empty<RoadSegmentWidthAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Widths.Take(1)
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Widths
                        .Select(widthAttribute =>
                            new RoadSegmentWidthAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = widthAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                                Width = widthAttribute.Width,
                                WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                                FromPosition = (double)widthAttribute.FromPosition,
                                ToPosition = (double)widthAttribute.ToPosition,
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
    public Task When_modifying_road_segment_geometry_with_new_widths_only()
    {
        return new ModifyRoadSegmentGeometryScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Widths.Length; i++)
                {
                    roadSegmentModified.Widths[i].AttributeId = i + 1 + roadSegmentModified.Widths.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = Array.Empty<RoadSegmentWidthAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Widths
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentGeometryModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentGeometryModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentGeometryModified.When)
                        }))
                    .Concat(context.RoadSegmentGeometryModified.Widths
                        .Select(widthAttribute =>
                            new RoadSegmentWidthAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = widthAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                                Width = widthAttribute.Width,
                                WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                                FromPosition = (double)widthAttribute.FromPosition,
                                ToPosition = (double)widthAttribute.ToPosition,
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
    public Task When_modifying_road_segments_with_new_widths_only()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Widths.Length; i++)
                {
                    roadSegmentModified.Widths[i].AttributeId = i + 1 + roadSegmentModified.Widths.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = Array.Empty<RoadSegmentWidthAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Widths
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentModified.When)
                        }))
                    .Concat(context.RoadSegmentModified.Widths
                        .Select(widthAttribute =>
                            new RoadSegmentWidthAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = widthAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                                Width = widthAttribute.Width,
                                WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                                FromPosition = (double)widthAttribute.FromPosition,
                                ToPosition = (double)widthAttribute.ToPosition,
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
    public Task When_modifying_road_segment_attributes_with_new_widths_only()
    {
        return new ModifyRoadSegmentAttributesScenarioBuilder(_fixture)
            .Setup((_, roadSegmentModified) =>
            {
                for (var i = 0; i < roadSegmentModified.Widths.Length; i++)
                {
                    roadSegmentModified.Widths[i].AttributeId = i + 1 + roadSegmentModified.Widths.Length;
                }
            })
            .Expect((context, roadSegmentVersion) =>
            {
                roadSegmentVersion.Widths = Array.Empty<RoadSegmentWidthAttributeVersion>()
                    .Concat(context.RoadSegmentAdded.Widths
                        .Select(widthAttribute => new RoadSegmentWidthAttributeVersion
                        {
                            Position = roadSegmentVersion.Position,
                            Id = widthAttribute.AttributeId,
                            RoadSegmentId = roadSegmentVersion.Id,
                            AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                            Width = widthAttribute.Width,
                            WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                            FromPosition = (double)widthAttribute.FromPosition,
                            ToPosition = (double)widthAttribute.ToPosition,
                            OrganizationId = context.AcceptedRoadSegmentAttributesModified.OrganizationId,
                            OrganizationName = context.AcceptedRoadSegmentAttributesModified.Organization,
                            IsRemoved = true,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(context.AcceptedRoadSegmentAttributesModified.When)
                        }))
                    .Concat(context.RoadSegmentAttributesModified.Widths
                        .Select(widthAttribute =>
                            new RoadSegmentWidthAttributeVersion
                            {
                                Position = roadSegmentVersion.Position,
                                Id = widthAttribute.AttributeId,
                                RoadSegmentId = roadSegmentVersion.Id,
                                AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                                Width = widthAttribute.Width,
                                WidthLabel = new RoadSegmentWidth(widthAttribute.Width).ToDutchString(),
                                FromPosition = (double)widthAttribute.FromPosition,
                                ToPosition = (double)widthAttribute.ToPosition,
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
