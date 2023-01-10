namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}
