namespace RoadRegistry.BackOffice.Messages;

public class UploadRoadNetworkChangesArchive
{
    public string ArchiveId { get; set; }
    public bool FeatureCompareCompleted { get; set; }
}
