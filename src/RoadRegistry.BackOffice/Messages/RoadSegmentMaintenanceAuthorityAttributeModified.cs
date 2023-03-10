namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;

public class RoadSegmentMaintenanceAuthorityAttributeModified : RoadSegmentAttributeModified
{
    public override string EventName => "RoadSegmentMaintenanceAuthorityModified";

    public OrganizationId MaintenanceAuthority { get; set; }

    public IReadOnlyCollection<RoadSegmentId> RoadSegmentIdentifiers { get; set; }

}
