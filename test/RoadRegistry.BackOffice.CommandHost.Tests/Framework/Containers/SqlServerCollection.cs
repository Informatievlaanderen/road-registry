namespace RoadRegistry.BackOffice.CommandHost.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}
