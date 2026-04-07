using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class SeedUploadStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO [RoadRegistryExtracts].[ExtractUploadStatusHistory] ([UploadId],[Timestamp],[Status])
SELECT [UploadId]
    ,[UploadedOn]
    ,[Status]
FROM [RoadRegistryExtracts].[ExtractUploads]
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
