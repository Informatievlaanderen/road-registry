namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}