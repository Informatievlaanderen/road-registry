namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Framework.Containers;
    using Framework.Projections;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.SqlServer.Types;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.GML2;
    using NetTopologySuite.Utilities;
    using Newtonsoft.Json;
    using Schema.RoadSegmentDenorm;
    using RoadRegistry.Projections;
    using Schema;
    using Xunit;
    using Assert = Xunit.Assert;
    using GeometryTranslator = BackOffice.Core.GeometryTranslator;
    using LineString = NetTopologySuite.Geometries.LineString;

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

        // [Fact]
        // public async Task ImportedRoadNodeExample_458()
        // {
        //     var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>("458");
        //
        //     var hexToBytes = WKBReader.HexToBytes(
        //         "8A7A000001070D000000FFC0CAA1AABD0E4100DBF97E37A00341FF490C02B1BD0E4100448B6C69A00341FF490C02B3BD0E417F52B81E79A00341015D8FC2B3BD0E41FF643BDF85A00341FF490C02B4BD0E41FF3F355E8AA00341FF490C02B1BD0E41FF643BDF9DA00341015D8FC2ABBD0E41FF643BDFB3A00341FEE5D022A7BD0E41FF643BDFC5A00341FF490C029EBD0E41FFB6F3FDDFA00341FE3789419DBD0E417F52B81EE2A00341015D8FC291BD0E41FF643BDF07A1034101F953E37DBD0E417FC976BE3EA10341FFC0CAA16BBD0E4180ED7C3F77A1034100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000DF652E43AF2A19404B27BD27028A204076D295E48DBB23400A17FB55BCDB24408D4CF7EBABCA2940B8C0C17D2D722F405BC57978CA0B32404330891933813540F4DEC3266CC93540D6C4B48641B83A4073BE2654E10141402ABB81C7F5B7444001000000010000000001000000FFFFFFFF0000000002");
        //
        //     var sqlGeometry = SqlGeometry.Deserialize(
        //         new SqlBytes(
        //             hexToBytes));
        //
        //     var coordinates = new Coordinate[sqlGeometry.STNumPoints().Value];
        //
        //     for (int i = 0; i < coordinates.Length; i++)
        //     {
        //         var stPointN = sqlGeometry.STPointN(i + 1);
        //         coordinates[i] = new CoordinateM(stPointN.STX.Value, stPointN.STY.Value, stPointN.M.Value);
        //     }
        //
        //     var multiLineString = GeometryConfiguration.GeometryFactory.CreateMultiLineString(
        //         new[]
        //         {
        //             GeometryConfiguration.GeometryFactory.CreateLineString(coordinates)
        //         });
        //
        //     var builderConstructedGeometry = BuilderConstructedGeometry(importedRoadSegment);
        //
        //     var asBinary = multiLineString.AsBinary();
        //     // var asBinaryZm = sqlGeometry.AsBinaryZM().Buffer;
        //     var oldBytes = sqlGeometry.Serialize().Buffer;
        //
        //     var hex1 = WKBWriter.ToHex(asBinary);
        //     // var s = WKBWriter.ToHex(asBinaryZm);
        //     var hex = WKBWriter.ToHex(oldBytes);
        //     var newBytes = builderConstructedGeometry.Serialize().Buffer;
        //
        //     var comparisonConfig = new ComparisonConfig()
        //     {
        //         CompareChildren = true,
        //         CompareFields = true,
        //         CompareProperties = true,
        //         CaseSensitive = true,
        //         MaxStructDepth = 5,
        //         MaxByteArrayDifferences = Int32.MaxValue,
        //         MaxDifferences = Int32.MaxValue,
        //         ComparePrivateFields = true,
        //         ComparePrivateProperties = true,
        //     };
        //     var comparisonResult = new CompareLogic(comparisonConfig).Compare(builderConstructedGeometry, sqlGeometry);
        //     var byteComparison = new CompareLogic(comparisonConfig).Compare(newBytes, oldBytes);
        //
        //
        //     Assert.Equal(newBytes, oldBytes);
        //
        //     var s1 = WKBWriter.ToHex(newBytes);
        //     var translatedGeometry = GeometryTranslator.Translate(importedRoadSegment.Geometry);
        //     ((LineString) translatedGeometry.Geometries[0]).GetPointN(0).M = 0.0;
        //
        //     var read = new WellKnownBinaryReader().Read(WKBReader.HexToBytes(
        //         "8A7A000001070D000000FFC0CAA1AABD0E4100DBF97E37A00341FF490C02B1BD0E4100448B6C69A00341FF490C02B3BD0E417F52B81E79A00341015D8FC2B3BD0E41FF643BDF85A00341FF490C02B4BD0E41FF3F355E8AA00341FF490C02B1BD0E41FF643BDF9DA00341015D8FC2ABBD0E41FF643BDFB3A00341FEE5D022A7BD0E41FF643BDFC5A00341FF490C029EBD0E41FFB6F3FDDFA00341FE3789419DBD0E417F52B81EE2A00341015D8FC291BD0E41FF643BDF07A1034101F953E37DBD0E417FC976BE3EA10341FFC0CAA16BBD0E4180ED7C3F77A1034100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000DF652E43AF2A19404B27BD27028A204076D295E48DBB23400A17FB55BCDB24408D4CF7EBABCA2940B8C0C17D2D722F405BC57978CA0B32404330891933813540F4DEC3266CC93540D6C4B48641B83A4073BE2654E10141402ABB81C7F5B7444001000000010000000001000000FFFFFFFF0000000002"));
        //     var geometry = new WellKnownBinaryReader().Read(WKBReader.HexToBytes(
        //         "01BA0B00000D000000FFC0CAA1AABD0E4100DBF97E37A0034100000000000000000000000000000000FF490C02B1BD0E4100448B6C69A003410000000000000000DF652E43AF2A1940FF490C02B3BD0E417F52B81E79A0034100000000000000004B27BD27028A2040015D8FC2B3BD0E41FF643BDF85A00341000000000000000076D295E48DBB2340FF490C02B4BD0E41FF3F355E8AA0034100000000000000000A17FB55BCDB2440FF490C02B1BD0E41FF643BDF9DA0034100000000000000008D4CF7EBABCA2940015D8FC2ABBD0E41FF643BDFB3A003410000000000000000B8C0C17D2D722F40FEE5D022A7BD0E41FF643BDFC5A0034100000000000000005BC57978CA0B3240FF490C029EBD0E41FFB6F3FDDFA0034100000000000000004330891933813540FE3789419DBD0E417F52B81EE2A003410000000000000000F4DEC3266CC93540015D8FC291BD0E41FF643BDF07A103410000000000000000D6C4B48641B83A4001F953E37DBD0E417FC976BE3EA10341000000000000000073BE2654E1014140FFC0CAA16BBD0E4180ED7C3F77A1034100000000000000002ABB81C7F5B74440"));
        //
        //     await new RoadSegmentRecordProjection(Encoding.UTF8)
        //         .Scenario()
        //         .Given(importedRoadSegment)
        //         .Expect(new RoadSegmentDenormRecord
        //         {
        //             Id = 458,
        //             BeginOperator = "",
        //             BeginOrganization = "AGIV",
        //             BeginTime = DateTime.Parse("2016-01-15 17:11:08.000"),
        //             BeginApplication = "Wegenregister-BLL",
        //
        //             Maintainer = "73109",
        //             MaintainerLabel = "Gemeente Voeren",
        //
        //             Method = 2,
        //             MethodLabel = "ingemeten",
        //
        //             Category = "-8",
        //             CategoryLabel = "niet gekend",
        //
        //             Geometry = translatedGeometry,
        //             Geometry2D = null,
        //             GeometryVersion = 2,
        //
        //             Morphology = 114,
        //             MorphologyLabel = "wandel- of fietsweg, niet toegankelijk voor andere voertuigen",
        //
        //             Status = 4,
        //             StatusLabel = "in gebruik",
        //
        //             AccessRestriction = 1,
        //             AccessRestrictionLabel = "openbare weg",
        //
        //             OrganizationLabel = "Agentschap voor Geografische Informatie Vlaanderen",
        //             RecordingDate = DateTime.Parse("2016-01-15 15:20:07.000"),
        //
        //             SourceId = null,
        //             SourceIdSource = null,
        //
        //             TransactionId = 359,
        //
        //             LeftSideMunicipality = 507,
        //             LeftSideStreetNameId = 123904,
        //             LeftSideStreetNameLabel = "Mot                                                                                                                             ",
        //
        //             RightSideMunicipality = 507,
        //             RightSideStreetNameId = 123904,
        //             RightSideStreetNameLabel = "Mot                                                                                                                             ",
        //
        //             RoadSegmentVersion = 2,
        //             BeginRoadNodeId = 764900,
        //             EndRoadNodeId = 916,
        //         });
        // }
                //
        // [Fact]
        // public async Task ImportedRoadNodeExample_904()
        // {
        //     var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>("904");
        //
        //     var hexToBytes = WKBReader.HexToBytes(
        //         "8A7A0000010705000000008941609992084180C0CAA1A5160941FFED7C3F71960841801283C07E1709410077BE9FF296084180490C0244140941011283C01197084100894160D4130941FF9BC420C1970841809BC420BE110941000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000030868695A7C5F40E63DE83F66D16C400ED13755F6A06E40155BEF1D13B5734001000000010000000001000000FFFFFFFF0000000002");
        //
        //     var sqlGeometry = SqlGeometry.Deserialize(
        //         new SqlBytes(
        //             hexToBytes));
        //
        //     var builderConstructedGeometry = BuilderConstructedGeometry(importedRoadSegment);
        //
        //     var oldBytes = sqlGeometry.Serialize().Buffer;
        //     var newBytes = builderConstructedGeometry.Serialize().Buffer;
        //
        //     var comparisonConfig = new ComparisonConfig()
        //     {
        //         CompareChildren = true,
        //         CompareFields = true,
        //         CompareProperties = true,
        //         CaseSensitive = true,
        //         MaxStructDepth = 5,
        //         MaxByteArrayDifferences = Int32.MaxValue,
        //         MaxDifferences = Int32.MaxValue,
        //         ComparePrivateFields = true,
        //         ComparePrivateProperties = true,
        //     };
        //     var comparisonResult = new CompareLogic(comparisonConfig).Compare(builderConstructedGeometry, sqlGeometry);
        //     var byteComparison = new CompareLogic(comparisonConfig).Compare(newBytes, oldBytes);
        //
        //     await new RoadSegmentRecordProjection(Encoding.UTF8)
        //         .Scenario()
        //         .Given(importedRoadSegment)
        //         .Expect(new RoadSegmentDenormRecord
        //         {
        //             Id = 904,
        //             BeginOperator = "-8",
        //             BeginOrganization = "AGIV",
        //             BeginTime = DateTime.Parse("2014-02-20 14:35:32.000"),
        //             BeginApplication = "-8",
        //
        //             Maintainer = "13003",
        //             MaintainerLabel = "Gemeente Balen",
        //
        //             Method = 2,
        //             MethodLabel = "ingemeten",
        //
        //             Category = "-8",
        //             CategoryLabel = "niet gekend",
        //
        //             Geometry = null,
        //             Geometry2D = null,
        //             GeometryVersion = 1,
        //
        //             Morphology = 114,
        //             MorphologyLabel = "wandel- of fietsweg, niet toegankelijk voor andere voertuigen",
        //
        //             Status = 4,
        //             StatusLabel = "in gebruik",
        //
        //             AccessRestriction = 1,
        //             AccessRestrictionLabel = "openbare weg",
        //
        //             OrganizationLabel = "Agentschap voor Geografische Informatie Vlaanderen",
        //             RecordingDate = DateTime.Parse("2014-02-20 14:35:32.237"),
        //             SourceId = null,
        //             TransactionId = 0,
        //
        //             LeftSideMunicipality = 46,
        //             LeftSideStreetNameId = -9,
        //             LeftSideStreetNameLabel = null,
        //
        //             RightSideMunicipality = 46,
        //             RightSideStreetNameId = -9,
        //             RightSideStreetNameLabel = null,
        //
        //             RoadSegmentVersion = 1,
        //             SourceIdSource = null,
        //             BeginRoadNodeId = 800780,
        //             EndRoadNodeId = 125446,
        //         });
        // }
    }
}
