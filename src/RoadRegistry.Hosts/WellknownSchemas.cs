namespace RoadRegistry.Hosts;

public static class WellknownSchemas
{
    public const string CommandHostSchema = "RoadRegistryBackOfficeCommandHost";
    public const string EditorMetaSchema = "RoadRegistryEditorMeta";
    public const string EditorSchema = "RoadRegistryEditor";
    public const string ExtractHostSchema = "RoadRegistryBackOfficeExtractHost";
    public const string EventHostSchema = "RoadRegistryBackOfficeEventHost";
    public const string EventSchema = "RoadRegistry";
    public const string ProductMetaSchema = "RoadRegistryProductMeta";
    public const string ProductSchema = "RoadRegistryProduct";
    public const string SnapshotSchema = "RoadRegistrySnapshot";
    public const string SyndicationMetaSchema = "RoadRegistrySyndicationMeta";
    public const string SyndicationSchema = "RoadRegistrySyndication";
    public const string WmsMetaSchema = "RoadRegistryWmsMeta";
    public const string WmsSchema = "RoadRegistryWms";
    public const string WfsMetaSchema = "RoadRegistryWfsMeta";
    public const string WfsSchema = "RoadRegistryWfs";
    public const string StreetNameConsumerSchema = "RoadRegistryStreetNameConsumer";

    public const string RoadNodeProducerSnapshotMetaSchema = "RoadRegistryRoadNodeProducerSnapshotMeta";
    public const string RoadNodeProducerSnapshotSchema = "RoadRegistryRoadNodeProducerSnapshot";

    public const string NationalRoadProducerSnapshotMetaSchema = "RoadRegistryNationalRoadProducerSnapshotMetaSchema";
    public const string NationalRoadProducerSnapshotSchema = "RoadRegistryNationalRoadProducerSnapshotSchema";
    
}

public static class MigrationTables
{
    public const string Syndication = "__EFMigrationsHistorySyndication";
    public const string Product = "__EFMigrationsHistoryProduct";
    public const string Editor = "__EFMigrationsHistoryEditor";
    public const string Wms = "__EFMigrationsHistoryWms";
    public const string Wfs = "__EFMigrationsHistoryWfs";
    public const string StreetNameConsumer = "__EFMigrationsHistoryStreetNameConsumer";

    public const string RoadNodeProducerSnapshot = "__EFMigrationsHistoryRoadNodeProducerSnapshot";
    public const string NationalRoadProducerSnapshot = "__EFMigrationsHistoryNationalRoadProducerSnapshot";
}
