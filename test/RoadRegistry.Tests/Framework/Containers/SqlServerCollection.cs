namespace RoadRegistry.Framework.Containers;

using Xunit;

[CollectionDefinition(nameof(SqlServerCollection))]
internal class SqlServerCollection : ICollectionFixture<SqlServer>
{
}
