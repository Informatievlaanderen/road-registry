namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using RoadSegment.ValueObjects;

public class RoadSegmentWidthAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentWidthAttributeRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeEuropeanRoadNumber();
        _fixture.CustomizeNationalRoadNumber();
        _fixture.CustomizeNumberedRoadNumber();
        _fixture.CustomizeRoadSegmentNumberedRoadDirection();
        _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeRoadSegmentGeometryVersion();

        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentLaneAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeRoadSegmentWidthAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_node_with_widths()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_node_without_widths()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>();

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(importedRoadSegment)
            .ExpectNone();
    }

    [Fact]
    public Task When_importing_road_nodes()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(segment =>
            {
                segment.Widths = _fixture
                    .CreateMany<ImportedRoadSegmentWidthAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Widths
                    .Select(width => new RoadSegmentWidthAttributeRecord
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                        {
                            WB_OIDN = { Value = width.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = segment.Id + "_" + width.AsOfGeometryVersion },
                            BREEDTE = { Value = width.Width },
                            VANPOS = { Value = (double)width.FromPosition },
                            TOTPOS = { Value = (double)width.ToPosition },
                            BEGINTIJD = { Value = width.Origin.Since },
                            BEGINORG = { Value = width.Origin.OrganizationId },
                            LBLBGNORG = { Value = width.Origin.Organization }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_nodes_with_modified_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Select(attributes =>
            {
                var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                roadSegmentWidthAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentWidthAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_removed_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = Array.Empty<RoadSegmentWidthAttributes>();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectNone();
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_added_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Append(_fixture.Create<RoadSegmentWidthAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_modified_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Select((attributes, i) =>
            {
                if (i % 2 == 0)
                {
                    var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                    roadSegmentWidthAttributes.AttributeId = attributes.AttributeId;
                    return roadSegmentWidthAttributes;
                }

                return attributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_removed_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Take(roadSegmentAdded.Widths.Length - 1)
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentGeometryModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, change =>
        {
            var segment = change.RoadSegmentGeometryModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition }, BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentGeometryModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentGeometryModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition }, BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentAttributesModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeRecord
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = width.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                    BREEDTE = { Value = width.Width },
                    VANPOS = { Value = (double)width.FromPosition },
                    TOTPOS = { Value = (double)width.ToPosition }, BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());

        return new RoadSegmentWidthAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .ExpectNone();
    }
}
