namespace RoadRegistry.Jobs.Processor.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}