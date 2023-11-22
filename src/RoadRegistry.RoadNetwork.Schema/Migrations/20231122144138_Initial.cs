using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.RoadNetwork.Schema.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "EuropeanRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "GradeSeparatedJunctionId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "NationalRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "NumberedRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "RoadNodeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "RoadSegmentId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "RoadSegmentLaneAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "RoadSegmentSurfaceAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "RoadSegmentWidthAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.CreateSequence<int>(
                name: "TransactionId",
                schema: "RoadNetwork");

            //TODO-rik add sql to set initial sequence values
            var sequenceDataSources = new Dictionary<string, string>
            {
                {"RoadSegmentId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegment]"},
                {"EuropeanRoadAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentEuropeanRoadAttribute]"},
                {"GradeSeparatedJunctionId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[GradeSeparatedJunction]"},
                {"NationalRoadAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentNationalRoadAttribute]"},
                {"NumberedRoadAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentNumberedRoadAttribute]"},
                {"RoadNodeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadNode]"},
                {"RoadSegmentLaneAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentLaneAttribute]"},
                {"RoadSegmentSurfaceAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentSurfaceAttribute]"},
                {"RoadSegmentWidthAttributeId", "SELECT MAX(Id) FROM [road-registry].[RoadRegistryEditor].[RoadSegmentWidthAttribute]"},
                {"TransactionId", "SELECT MAX([TransactionId]) FROM [road-registry-events].[RoadRegistryRoadSegmentProducerSnapshot].[RoadSegment]"},
            };

            foreach (var sequenceDataSource in sequenceDataSources)
            {
                migrationBuilder.Sql($@"
DECLARE @sth bigint;
SET @sth = SELECT COALESCE(({sequenceDataSource.Value}), 0) + 1;
DECLARE @sql nvarchar(max);
SET @sql = N'ALTER SEQUENCE [RoadNetwork].{sequenceDataSource.Key} RESTART WITH ' + cast(@sth as nvarchar(20)) + ';';
EXEC SP_EXECUTESQL @sql;
");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "EuropeanRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "GradeSeparatedJunctionId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "NationalRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "NumberedRoadAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "RoadNodeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "RoadSegmentId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "RoadSegmentLaneAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "RoadSegmentSurfaceAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "RoadSegmentWidthAttributeId",
                schema: "RoadNetwork");

            migrationBuilder.DropSequence(
                name: "TransactionId",
                schema: "RoadNetwork");
        }
    }
}
