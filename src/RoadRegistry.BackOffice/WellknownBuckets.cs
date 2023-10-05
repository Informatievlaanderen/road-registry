namespace RoadRegistry.BackOffice;

public static class WellknownBuckets
{
    public const string ExtractDownloadsBucket = "ExtractDownloads";
    public const string FeatureCompareBucket = "FeatureCompare";
    public const string ImportLegacyBucket = "ImportLegacy";
    public const string UploadsBucket = "Uploads";
    public const string SqsMessagesBucket = "SqsMessages";
    public const string SnapshotsBucket = "Snapshots";
}

public static class WellknownQueues
{
    public const string AdminQueue = "Admin";
    public const string BackOfficeQueue = "BackOffice";
    public const string SnapshotQueue = "Snapshot";
}
