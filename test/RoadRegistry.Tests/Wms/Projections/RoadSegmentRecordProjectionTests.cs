namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework;
    using RoadRegistry.Framework.Containers;
    using RoadRegistry.Framework.Projections;
    using RoadRegistry.Projections;
    using Schema;
    using Xunit;
    using Assert = Xunit.Assert;

    [Collection(nameof(SqlServerCollection))]
    public class RoadSegmentRecordProjectionTests
    {
        private readonly SqlServer _sqlServer;
        private readonly Fixture _fixture;
        private readonly TestDataHelper _testDataHelper;

        public RoadSegmentRecordProjectionTests(SqlServer sqlServer)
        {
            _sqlServer = sqlServer ?? throw new ArgumentNullException(nameof(sqlServer));
            _testDataHelper = new TestDataHelper();

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

        [Theory]
        [InlineData(904)]
        [InlineData(458)]
        public async Task GeometryTest(int wegSegmentId)
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

            var sqlGeometry = SqlGeometryTranslator.TranslateGeometry(importedRoadSegment);

            var expected = await _testDataHelper.ExpectedWegsegmentDeNormFromFileAsync(wegSegmentId);

            Assert.Equal(expected.Geometrie, sqlGeometry.Serialize().Buffer);
        }

        [Theory]
        [InlineData(904)]
        [InlineData(458)]
        public async Task Geometry2DTest(int wegSegmentId)
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

            var sqlGeometry = SqlGeometryTranslator.TranslateGeometry2D(importedRoadSegment);

            var expected = await _testDataHelper.ExpectedWegsegmentDeNormFromFileAsync(wegSegmentId);

            Assert.Equal(expected.Geometrie2D, sqlGeometry.Serialize().Buffer);
        }

        [Theory]
        [InlineData(904)]
        [InlineData(458)]
        public async Task CanWriteAndReadSameGeometry(int wegSegmentId)
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

            var sqlGeometry = SqlGeometryTranslator.TranslateGeometry(importedRoadSegment);

            var expected = await _testDataHelper.ExpectedWegsegmentDeNormFromFileAsync(wegSegmentId);

            var databaseAsync = await _sqlServer.CreateDatabaseAsync();

            await _sqlServer.EnsureWmsSchemaAsync(databaseAsync);

            using (var conn = new SqlConnection(databaseAsync.ConnectionString))
            {
                conn.Open();

                const string sql =
                    "INSERT INTO [dbo].[wegsegmentDeNorm](wegSegmentId, geometrie) VALUES(@param1,@param2)";
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add("@param1", SqlDbType.Int).Value = wegSegmentId;
                    cmd.Parameters.Add("@param2", SqlDbType.Binary).Value = sqlGeometry.Serialize().Buffer;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "select wegSegmentId, geometrie from [dbo].[wegsegmentDeNorm] where wegSegmentId = @param1";
                    cmd.Parameters.Add("@param1", SqlDbType.Int).Value = wegSegmentId;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var geoValue = reader.GetSqlBytes(1);
                            Assert.Equal(
                                geoValue.Buffer,
                                expected.Geometrie);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task ImportedRoadNodeExample_458()
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(458);

            var expectedGeometry = await _testDataHelper.ExpectedGeometry(458);
            var expectedGeometry2D = await _testDataHelper.ExpectedGeometry2D(458);

            await new RoadSegmentRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentRecord
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

                    Geometry = expectedGeometry,
                    Geometry2D = expectedGeometry2D,
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

        [Fact]
        public async Task ImportedRoadNodeExample_904()
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(904);

            var expectedGeometry = await _testDataHelper.ExpectedGeometry(904);
            var expectedGeometry2D = await _testDataHelper.ExpectedGeometry2D(904);

            await new RoadSegmentRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentRecord
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

                    Geometry = expectedGeometry,
                    Geometry2D = expectedGeometry2D,
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
        public async Task ImportedRoadNodeExample_4()
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(4);

            var expectedGeometry = await _testDataHelper.ExpectedGeometry(4);
            var expectedGeometry2D = await _testDataHelper.ExpectedGeometry2D(4);

            await new RoadSegmentRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentRecord
                {
                    Id = 4,
                    BeginOperator = "",
                    BeginOrganization = "AWV",
                    BeginTime = DateTime.Parse("2017-03-15 15:44:48.000"),
                    BeginApplication = "Wegenregister-BLL",

                    Maintainer = "AWV720",
                    MaintainerLabel = "Agentschap Wegen en Verkeer - District Centraal-Limburg",

                    Method = 2,
                    MethodLabel = "ingemeten",

                    Category = "-9",
                    CategoryLabel = "niet van toepassing",

                    Geometry = expectedGeometry,
                    Geometry2D = expectedGeometry2D,
                    GeometryVersion = 3,

                    Morphology = 114,
                    MorphologyLabel = "dienstweg",

                    Status = 4,
                    StatusLabel = "in gebruik",

                    AccessRestriction = 1,
                    AccessRestrictionLabel = "openbare weg",

                    OrganizationLabel = "Agentschap Wegen en Verkeer",
                    RecordingDate = DateTime.Parse("2017-03-15 15:44:11.000"),
                    SourceId = null,
                    TransactionId = 979,

                    LeftSideMunicipality = 503,
                    LeftSideStreetNameId = -9,
                    LeftSideStreetNameLabel = null,

                    RightSideMunicipality = 503,
                    RightSideStreetNameId = -9,
                    RightSideStreetNameLabel = null,

                    RoadSegmentVersion = 3,
                    SourceIdSource = null,
                    BeginRoadNodeId = 7,
                    EndRoadNodeId = 8,
                });
        }
    }
}
