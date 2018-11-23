namespace RoadRegistry.Api.Tests.Framework
{
    using Xunit;

    [CollectionDefinition(nameof(SqlServerDatabaseCollection))]
    public class SqlServerDatabaseCollection : ICollectionFixture<SqlServerDatabaseFixture>
    {
    }
}
