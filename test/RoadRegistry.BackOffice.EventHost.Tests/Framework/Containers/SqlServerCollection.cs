namespace RoadRegistry.BackOffice.EventHost.Tests.Framework.Containers;

using Xunit;

[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServer>
{
}
