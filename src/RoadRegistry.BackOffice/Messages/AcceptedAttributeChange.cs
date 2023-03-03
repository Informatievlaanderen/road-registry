namespace RoadRegistry.BackOffice.Messages;

public class AcceptedAttributeChange
{
    public Problem[] Problems { get; set; }

    public RoadSegmentStatusModified RoadSegmentStatusModified { get; set; }
    public RoadSegmentMorphologyModified RoadSegmentMorphologyModified { get; set; }
    public RoadSegmentAccessRestrictionModified RoadSegmentAccessRestrictionModified { get; set; }
    public RoadSegmentMaintenanceAuthorityModified RoadSegmentMaintenanceAuthorityModified { get; set; }
    public RoadSegmentCategoryModified RoadSegmentCategoryModified { get; set; }
}
