namespace RoadRegistry.Syndication.ProjectionHost.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}
