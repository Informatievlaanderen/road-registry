namespace RoadRegistry.Jobs.Processor.Upload.Tests.Framework.Containers;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}