namespace RoadRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "RoadRegistry";
        public const string Shape = "RoadRegistryShape";
        public const string Oslo = "RoadRegistryOslo";

        public const string ProjectionMetaData = "RoadRegistryProjectionMetaData";
    }

    public class MigrationTables
    {
        public const string Oslo = "__EFMigrationsHistoryOslo";
        public const string Shape = "__EFMigrationsHistoryShape";
    }
}
