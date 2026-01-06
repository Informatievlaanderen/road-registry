namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentSurfaceAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentSurfaceAttributeRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = FixtureFactory.Create();
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
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeRoadSegmentSurfaceAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_node_with_surfaces()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_node_without_surfaces()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>();

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
                segment.Surfaces = _fixture
                    .CreateMany<ImportedRoadSegmentSurfaceAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Surfaces
                    .Select(surface => new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = surface.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                            TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                            LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                            VANPOS = { Value = (double)surface.FromPosition },
                            TOTPOS = { Value = (double)surface.ToPosition },
                            BEGINTIJD = { Value = surface.Origin.Since },
                            BEGINORG = { Value = surface.Origin.OrganizationId },
                            LBLBGNORG = { Value = surface.Origin.Organization }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_nodes_with_modified_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select(attributes =>
            {
                var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentSurfaceAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_removed_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectNone();
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_added_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Append(_fixture.Create<RoadSegmentSurfaceAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_modified_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select((attributes, i) =>
            {
                if (i % 2 == 0)
                {
                    var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                    roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                    return roadSegmentSurfaceAttributes;
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

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes_with_some_removed_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Take(roadSegmentAdded.Surfaces.Length - 1)
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_surfaces_only()
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

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentGeometryModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentGeometryModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_surfaces_only()
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

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_surfaces_only()
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

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeRecord
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = surface.AttributeId },
                    WS_OIDN = { Value = segment.Id },
                    WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                    TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                    LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                    VANPOS = { Value = (double)surface.FromPosition },
                    TOTPOS = { Value = (double)surface.ToPosition },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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

        return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .ExpectNone();
    }
}
