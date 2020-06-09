namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
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

            _fixture.CustomizeArchiveId();

            _fixture.CustomizeImportedRoadSegment();
            _fixture.CustomizeRoadSegmentAdded();
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

        [Theory]
        [InlineData(904)]
        [InlineData(458)]
        [InlineData(4)]
        public async Task When_importing_road_segments(int wegSegmentId)
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

            var expectedGeometry = _testDataHelper.ExpectedGeometry(wegSegmentId);
            var expectedGeometry2D = _testDataHelper.ExpectedGeometry2D(wegSegmentId);

            var expectedRoadSegment = _testDataHelper.ExpectedRoadSegment(wegSegmentId);

            await new RoadSegmentRecordProjection()
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentRecord
                {
                    Id = expectedRoadSegment.wegsegmentID,
                    BeginOperator = expectedRoadSegment.beginoperator,
                    BeginOrganization = expectedRoadSegment.beginorganisatie,
                    BeginTime = expectedRoadSegment.begintijd,
                    BeginApplication = expectedRoadSegment.beginapplicatie,

                    Maintainer = expectedRoadSegment.beheerder,
                    MaintainerLabel = expectedRoadSegment.lblBeheerder,

                    Method = expectedRoadSegment.methode,
                    MethodLabel = expectedRoadSegment.lblMethode,

                    Category = expectedRoadSegment.categorie,
                    CategoryLabel = expectedRoadSegment.lblCategorie,

                    Geometry = expectedGeometry,
                    Geometry2D = expectedGeometry2D,
                    GeometryVersion = expectedRoadSegment.geometrieversie,

                    Morphology = expectedRoadSegment.morfologie,
                    MorphologyLabel = expectedRoadSegment.lblMorfologie,

                    Status = expectedRoadSegment.status,
                    StatusLabel = expectedRoadSegment.lblStatus,

                    AccessRestriction = expectedRoadSegment.toegangsbeperking,
                    AccessRestrictionLabel = expectedRoadSegment.lblToegangsbeperking,

                    OrganizationLabel = expectedRoadSegment.lblOrganisatie,
                    RecordingDate = expectedRoadSegment.opnamedatum,

                    SourceId = expectedRoadSegment.sourceID,
                    SourceIdSource = expectedRoadSegment.bronSourceID,

                    TransactionId = expectedRoadSegment.transactieID,

                    LeftSideMunicipality = null,
                    LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                    LeftSideStreetNameLabel = expectedRoadSegment.linksStraatnaam,

                    RightSideMunicipality = null,
                    RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                    RightSideStreetNameLabel = expectedRoadSegment.linksStraatnaam,

                    RoadSegmentVersion = expectedRoadSegment.wegsegmentversie,
                    BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
                    EndRoadNodeId = expectedRoadSegment.eindWegknoopID,
                });
        }

        [Fact]
        public Task When_adding_road_segments()
        {
            var message = _fixture.Create<RoadNetworkChangesBasedOnArchiveAccepted>();
            var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var segment = change.RoadSegmentAdded;
                return (object)new RoadSegmentRecord
                {
                    Id = segment.Id,
                    BeginOperator = message.Operator,
                    BeginOrganization = message.Organization,
                    BeginTime = DateTime.Parse(message.When),
                    BeginApplication = null,

                    Maintainer = segment.MaintenanceAuthority.Code,
                    MaintainerLabel = segment.MaintenanceAuthority.Name,

                    Method = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
                    MethodLabel = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                    Category = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
                    CategoryLabel = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                    Geometry = WmsGeometryTranslator.Translate3D(segment.Geometry),
                    Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
                    GeometryVersion = segment.GeometryVersion,

                    Morphology = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
                    MorphologyLabel = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                    Status = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
                    StatusLabel = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                    AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
                    AccessRestrictionLabel = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                    OrganizationLabel = message.Organization,
                    RecordingDate = DateTime.Parse(message.When),

                    SourceId = null,
                    SourceIdSource = null,

                    TransactionId = null,

                    LeftSideMunicipality = null,
                    LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                    LeftSideStreetNameLabel = null,

                    RightSideMunicipality = null,
                    RightSideStreetNameId = segment.RightSide.StreetNameId,
                    RightSideStreetNameLabel = null,

                    RoadSegmentVersion = segment.Version,
                    BeginRoadNodeId = segment.StartNodeId,
                    EndRoadNodeId = segment.EndNodeId,
                };
            });

            return new RoadSegmentRecordProjection()
                .Scenario()
                .Given(message)
                .Expect(expectedRecords);
        }
    }
}
