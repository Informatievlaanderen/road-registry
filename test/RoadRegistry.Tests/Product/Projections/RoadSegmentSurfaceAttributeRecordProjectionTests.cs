namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework.Projections;
    using RoadRegistry.Projections;
    using Schema.RoadSegments;
    using Xunit;

    public class RoadSegmentSurfaceAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentSurfaceAttributeRecordProjectionTests(ProjectionTestServices services)
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

            _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
            _fixture.CustomizeRoadSegmentSurfaceAttributes();
            _fixture.CustomizeRoadSegmentAdded();
            _fixture.CustomizeRoadSegmentModified();
            _fixture.CustomizeRoadSegmentRemoved();
            _fixture.CustomizeRoadNetworkChangesAccepted();
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
                                TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                                LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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
        public Task When_importing_a_road_node_without_surfaces()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Surfaces = new ImportedRoadSegmentSurfaceAttribute[0];

            return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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
            roadSegmentModified.Surfaces = new RoadSegmentSurfaceAttributes[0];

            var acceptedRoadSegmentModified = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(roadSegmentModified);

            return new RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
                .ExpectNone();
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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

                return segment.Surfaces.Select(surface => (object) new RoadSegmentSurfaceAttributeRecord
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = surface.AttributeId },
                        WS_OIDN = { Value = segment.Id },
                        WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                        TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                        LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
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
}
