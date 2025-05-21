namespace RoadRegistry.Tests.Debugging;

using System.Text;
using Editor.Schema;
using Editor.Schema.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Product.Schema;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

public class FixCategoryInEditorAndProductDbfRecords
{
    public FixCategoryInEditorAndProductDbfRecords(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    private IConfiguration Configuration { get; }
    private ITestOutputHelper TestOutputHelper { get; }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task Run()
    {
        //const DbEnvironment env = DbEnvironment.TST;
        const DbEnvironment env = DbEnvironment.STG;
        // const DbEnvironment env = DbEnvironment.PRD;

        var category = RoadSegmentCategory.LocalAccessRoad;

        await FixEditorProjections(env, category);
        await FixProductProjections(env, category);
    }

    private string GetEditorProjectionsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"EditorProjections-{environment}") ?? Configuration.GetRequiredConnectionString("EditorProjections");
    }

    private async Task FixEditorProjections(DbEnvironment environment, RoadSegmentCategory category)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var connectionString = GetEditorProjectionsConnectionString(environment);

        var dbContext = new EditorContext(new DbContextOptionsBuilder<EditorContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        var manager = new RecyclableMemoryStreamManager();
        var encoding = WellKnownEncodings.WindowsAnsi;

        var segments = dbContext.RoadSegments.Where(x => x.CategoryId == category.Translation.Identifier).ToList();
        foreach (var segment in segments)
        {
            var dbf = new RoadSegmentDbaseRecord().FromBytes(segment.DbaseRecord, manager, encoding);
            if (dbf.LBLWEGCAT.Value != category.Translation.Name)
            {
                dbf.LBLWEGCAT.Value = category.Translation.Name;

                segment.DbaseRecord = dbf.ToBytes(manager, encoding);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task FixProductProjections(DbEnvironment environment, RoadSegmentCategory category)
    {
        var connectionString = GetEditorProjectionsConnectionString(environment);

        var dbContext = new ProductContext(new DbContextOptionsBuilder<ProductContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        var manager = new RecyclableMemoryStreamManager();
        var encoding = WellKnownEncodings.WindowsAnsi;

        var segments = dbContext.RoadSegments.Where(x => x.CategoryId == category.Translation.Identifier).ToList();
        foreach (var segment in segments)
        {
            var dbf = new RoadSegmentDbaseRecord().FromBytes(segment.DbaseRecord, manager, encoding);
            if (dbf.LBLWEGCAT.Value != category.Translation.Name)
            {
                dbf.LBLWEGCAT.Value = category.Translation.Name;

                segment.DbaseRecord = dbf.ToBytes(manager, encoding);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private enum DbEnvironment
    {
        DEV,
        TST,
        STG,
        PRD
    }
}
