namespace RoadRegistry.Framework
{
    using Xunit;

    [CollectionDefinition(nameof(SqlServerCollection))]
    public class SqlServerCollection : ICollectionFixture<SqlServer>
    {
    }
}