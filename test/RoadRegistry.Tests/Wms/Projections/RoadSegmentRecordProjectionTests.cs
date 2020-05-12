namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework.Projections;
    using Newtonsoft.Json;
    using Schema.RoadSegmentDenorm;
    using RoadRegistry.Projections;
    using Xunit;

    public class RoadSegmentRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
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
        }

        [Fact]
        public Task ImportedRoadNodeExample_904()
        {
            var json = File.ReadAllText("Wms/Projections/TestData/importedRoadSegment.904.json");

            var importedRoadSegment = JsonConvert.DeserializeObject<ImportedRoadSegment>(json);

            return new RoadSegmentRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentDenormRecord
                {
                    Id = 904,
                    BeginOperator = "-8",
                    BeginOrganization = "AGIV",
                    BeginTime = DateTime.Parse("2014-02-20 14:35:32.000"),
                    BeginApplication = "-8",

                    Maintainer = "13003",
                    MaintainerLabel = "Gemeente Balen",

                    Method = 2,
                    MethodLabel = "ingemeten",

                    Category = "-8",
                    CategoryLabel = "niet gekend",

                    Geometry = null,
                    Geometry2D = null,
                    GeometryVersion = 1,

                    Morphology = 114,
                    MorphologyLabel = "wandel- of fietsweg, niet toegankelijk voor andere voertuigen",

                    Status = 4,
                    StatusLabel = "in gebruik",

                    AccessRestriction = 1,
                    AccessRestrictionLabel = "openbare weg",

                    OrganizationLabel = "Agentschap voor Geografische Informatie Vlaanderen",
                    RecordingDate = DateTime.Parse("2014-02-20 14:35:32.237"),
                    SourceId = null,
                    TransactionId = 0,

                    LeftSideMunicipality = 46,
                    LeftSideStreetNameId = -9,
                    LeftSideStreetNameLabel = null,

                    RightSideMunicipality = 46,
                    RightSideStreetNameId = -9,
                    RightSideStreetNameLabel = null,

                    RoadSegmentVersion = 1,
                    SourceIdSource = null,
                    BeginRoadNodeId = 800780,
                    EndRoadNodeId = 125446,
                });
        }

        [Fact]
        public Task ImportedRoadNodeExample_458()
        {
            var json = File.ReadAllText("Wms/Projections/TestData/importedRoadSegment.458.json");

            var importedRoadSegment = JsonConvert.DeserializeObject<ImportedRoadSegment>(json);

            return new RoadSegmentRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentDenormRecord
                {
                    Id = 458,
                    BeginOperator = "",
                    BeginOrganization = "AGIV",
                    BeginTime = DateTime.Parse("2016-01-15 17:11:08.000"),
                    BeginApplication = "Wegenregister-BLL",

                    Maintainer = "73109",
                    MaintainerLabel = "Gemeente Voeren",

                    Method = 2,
                    MethodLabel = "ingemeten",

                    Category = "-8",
                    CategoryLabel = "niet gekend",

                    Geometry = null,
                    Geometry2D = null,
                    GeometryVersion = 2,

                    Morphology = 114,
                    MorphologyLabel = "wandel- of fietsweg, niet toegankelijk voor andere voertuigen",

                    Status = 4,
                    StatusLabel = "in gebruik",

                    AccessRestriction = 1,
                    AccessRestrictionLabel = "openbare weg",

                    OrganizationLabel = "Agentschap voor Geografische Informatie Vlaanderen",
                    RecordingDate = DateTime.Parse("2016-01-15 15:20:07.000"),

                    SourceId = null,
                    SourceIdSource = null,

                    TransactionId = 359,

                    LeftSideMunicipality = 507,
                    LeftSideStreetNameId = 123904,
                    LeftSideStreetNameLabel = "Mot                                                                                                                             ",

                    RightSideMunicipality = 507,
                    RightSideStreetNameId = 123904,
                    RightSideStreetNameLabel = "Mot                                                                                                                             ",

                    RoadSegmentVersion = 2,
                    BeginRoadNodeId = 764900,
                    EndRoadNodeId = 916,
                });
        }
    }
}
