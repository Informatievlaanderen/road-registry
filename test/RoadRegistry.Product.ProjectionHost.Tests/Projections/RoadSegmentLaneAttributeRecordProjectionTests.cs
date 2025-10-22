namespace RoadRegistry.Product.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Product.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using RoadSegment.ValueObjects;

public class RoadSegmentLaneAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentLaneAttributeRecordProjectionTests(ProjectionTestServices services)
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

        _fixture.CustomizeRoadSegmentLaneAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_node_with_lanes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_node_without_lanes()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>();

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(importedRoadSegment)
            .Expect();
    }

    [Fact]
    public Task When_importing_road_nodes()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(segment =>
            {
                segment.Lanes = _fixture
                    .CreateMany<ImportedRoadSegmentLaneAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Lanes
                    .Select(lane => new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = { Value = lane.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                            AANTAL = { Value = lane.Count },
                            RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                            LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                            VANPOS = { Value = (double)lane.FromPosition },
                            TOTPOS = { Value = (double)lane.ToPosition },
                            BEGINTIJD = { Value = lane.Origin.Since },
                            BEGINORG = { Value = lane.Origin.OrganizationId },
                            LBLBGNORG = { Value = lane.Origin.Organization }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_nodes_with_modified_lanes_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Select(attributes =>
            {
                var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                roadSegmentLaneAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentLaneAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_removed_lanes_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = Array.Empty<RoadSegmentLaneAttributes>();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectNone();
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_added_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Append(_fixture.Create<RoadSegmentLaneAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_modified_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Select((attributes, i) =>
            {
                if (i % 2 == 0)
                {
                    var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                    roadSegmentLaneAttributes.AttributeId = attributes.AttributeId;
                    return roadSegmentLaneAttributes;
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_removed_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Take(roadSegmentAdded.Lanes.Length - 1)
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_lanes_only()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentGeometryModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentGeometryModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_lanes_only()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_lanes_only()
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

            return segment.Lanes?.Select(lane => (object)new RoadSegmentLaneAttributeRecord
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = lane.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                    AANTAL = { Value = lane.Count },
                    RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                    VANPOS = { Value = (double)lane.FromPosition },
                    TOTPOS = { Value = (double)lane.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            }) ?? [];
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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

        return new RoadSegmentLaneAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .ExpectNone();
    }
}
