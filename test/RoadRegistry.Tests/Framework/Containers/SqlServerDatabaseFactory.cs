namespace RoadRegistry.Tests.Framework.Containers;

public static class SqlServerDatabaseFactory
{
    public static ISqlServerDatabase Create(RoadRegistryAssembly assembly)
    {
        if (Environment.GetEnvironmentVariable("CI") == null)
        {
            return new SqlServerEmbeddedContainer(assembly);
        }

        return new SqlServerComposedContainer(assembly);
    }
}