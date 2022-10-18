namespace RoadRegistry.BackOffice.Api.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}