namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAttributesModified : IHaveHash
{
    public const string EventName = "RoadSegmentAttributesModified";

    public RoadSegmentMaintenanceAuthorityAttributeModified MaintenanceAuthority { get; set; }
    public RoadSegmentStatusAttributeModified Status { get; set; }
    public RoadSegmentMorphologyAttributeModified Morphology { get; set; }
    public RoadSegmentAccessRestrictionAttributeModified AccessRestriction { get; set; }
    public RoadSegmentCategoryAttributeModified Category { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
