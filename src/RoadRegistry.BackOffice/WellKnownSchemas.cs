namespace RoadRegistry.BackOffice;

public static class WellKnownSchemas
{
    public const string CommandHostSchema = "RoadRegistryBackOfficeCommandHost";
    public const string EditorMetaSchema = "RoadRegistryEditorMeta";
    public const string EditorMetricsSchema = "RoadRegistryEditorMetrics";
    public const string EditorSchema = "RoadRegistryEditor";
    public const string ExtractHostSchema = "RoadRegistryBackOfficeExtractHost";
    public const string EventHostSchema = "RoadRegistryBackOfficeEventHost";
    public const string EventSchema = "RoadRegistry";
    public const string ProductMetaSchema = "RoadRegistryProductMeta";
    public const string ProductMetricsSchema = "RoadRegistryProductMetrics";
    public const string ProductSchema = "RoadRegistryProduct";
    public const string SnapshotSchema = "RoadRegistrySnapshot";
    public const string SyndicationMetaSchema = "RoadRegistrySyndicationMeta";
    public const string SyndicationSchema = "RoadRegistrySyndication";
    public const string WmsMetaSchema = "RoadRegistryWmsMeta";
    public const string WmsSchema = "RoadRegistryWms";
    public const string WfsMetaSchema = "RoadRegistryWfsMeta";
    public const string WfsSchema = "RoadRegistryWfs";
    public const string StreetNameSchema = "RoadRegistryStreetName";
    public const string StreetNameSnapshotConsumerSchema = "RoadRegistryStreetNameSnapshotConsumer";
    public const string OrganizationConsumerSchema = "RoadRegistryOrganizationConsumer";

    public const string RoadNodeProducerSnapshotMetaSchema = "RoadRegistryRoadNodeProducerSnapshotMeta";
    public const string RoadNodeProducerSnapshotSchema = "RoadRegistryRoadNodeProducerSnapshot";
    public const string RoadSegmentProducerSnapshotMetaSchema = "RoadRegistryRoadSegmentProducerSnapshotMeta";
    public const string RoadSegmentProducerSnapshotSchema = "RoadRegistryRoadSegmentProducerSnapshot";
    public const string NationalRoadProducerSnapshotMetaSchema = "RoadRegistryNationalRoadProducerSnapshotMetaSchema";
    public const string NationalRoadProducerSnapshotSchema = "RoadRegistryNationalRoadProducerSnapshotSchema";
    public const string GradeSeparatedJunctionProducerSnapshotMetaSchema = "RoadRegistryGradeSeparatedJunctionProducerSnapshotMetaSchema";
    public const string GradeSeparatedJunctionProducerSnapshotSchema = "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema";
    public const string RoadSegmentSurfaceProducerSnapshotMetaSchema = "RoadRegistryRoadSegmentSurfaceProducerSnapshotMetaSchema";
    public const string RoadSegmentSurfaceProducerSnapshotSchema = "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema";
}

public static class MigrationTables
{
    public const string Syndication = "__EFMigrationsHistorySyndication";
    public const string Product = "__EFMigrationsHistoryProduct";
    public const string Editor = "__EFMigrationsHistoryEditor";
    public const string Wms = "__EFMigrationsHistoryWms";
    public const string Wfs = "__EFMigrationsHistoryWfs";
    public const string StreetName = "__EFMigrationsHistoryStreetName";
    public const string StreetNameSnapshotConsumer = "__EFMigrationsHistoryStreetNameSnapshotConsumer";
    public const string OrganizationConsumer = "__EFMigrationsHistoryOrganizationConsumer";

    public const string RoadNodeProducerSnapshot = "__EFMigrationsHistoryRoadNodeProducerSnapshot";
    public const string RoadSegmentProducerSnapshot = "__EFMigrationsHistoryRoadSegmentProducerSnapshot";
    public const string NationalRoadProducerSnapshot = "__EFMigrationsHistoryNationalRoadProducerSnapshot";
    public const string GradeSeparatedJunctionProducerSnapshot = "__EFMigrationsHistoryGradeSeparatedJunctionProducerSnapshot";
    public const string RoadSegmentSurfaceProducerSnapshot = "__EFMigrationsHistoryRoadSegmentSurfaceProducerSnapshot";
}
