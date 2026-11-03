namespace RoadRegistry.Projector.Tests.Projections;

using FluentAssertions;
using RoadRegistry.Projector.Projections;

// The rebuild drops the views that serve a read model, truncates underneath them and puts them back. Everything about
// that comes out of the server - the view bodies out of sys.sql_modules, the indexes out of sys.indexes - except the
// CREATE INDEX statements, which have to be written back from the catalog rows. That is the one piece with a shape
// worth asserting, and the one place a wrong string would silently leave an indexed view unindexed.
public class ViewIndexStatementTests
{
    private static SqlServerViewMaintenance.ServingView View()
        => new("wfs", "Wegsegmenten", "CREATE VIEW [wfs].[Wegsegmenten] WITH SCHEMABINDING AS SELECT 1 AS [ObjectId]");

    [Fact]
    public void AUniqueClusteredIndexOnOneColumnComesBackAsItWent()
    {
        var index = new SqlServerViewMaintenance.ViewIndex("IX_Wegsegmenten_ObjectId", IsUnique: true, IsClustered: true);
        index.KeyColumns.Add(new SqlServerViewMaintenance.ViewIndexColumn("ObjectId", IsDescending: false));

        SqlServerViewMaintenance.CreateIndexStatement(View(), index)
            .Should().Be("CREATE UNIQUE CLUSTERED INDEX [IX_Wegsegmenten_ObjectId] ON [wfs].[Wegsegmenten] ([ObjectId] ASC);");
    }

    [Fact]
    public void KeyColumnsKeepTheirOrderAndDirection()
    {
        var index = new SqlServerViewMaintenance.ViewIndex("IX_Two", IsUnique: false, IsClustered: false);
        index.KeyColumns.Add(new SqlServerViewMaintenance.ViewIndexColumn("First", IsDescending: false));
        index.KeyColumns.Add(new SqlServerViewMaintenance.ViewIndexColumn("Second", IsDescending: true));

        SqlServerViewMaintenance.CreateIndexStatement(View(), index)
            .Should().Be("CREATE NONCLUSTERED INDEX [IX_Two] ON [wfs].[Wegsegmenten] ([First] ASC, [Second] DESC);");
    }

    [Fact]
    public void IncludedColumnsAreNotKeyColumns()
    {
        var index = new SqlServerViewMaintenance.ViewIndex("IX_Covering", IsUnique: false, IsClustered: false);
        index.KeyColumns.Add(new SqlServerViewMaintenance.ViewIndexColumn("ObjectId", IsDescending: false));
        index.IncludedColumns.Add("Creatie");
        index.IncludedColumns.Add("Versie");

        SqlServerViewMaintenance.CreateIndexStatement(View(), index)
            .Should().Be("CREATE NONCLUSTERED INDEX [IX_Covering] ON [wfs].[Wegsegmenten] ([ObjectId] ASC) INCLUDE ([Creatie], [Versie]);");
    }
}
