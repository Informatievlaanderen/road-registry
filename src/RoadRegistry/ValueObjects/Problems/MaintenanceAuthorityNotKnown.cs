namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class MaintenanceAuthorityNotKnown : Error
{
    public static class ParameterName
    {
        public const string OrganizationId = "OrganizationId";
    }

    public MaintenanceAuthorityNotKnown(OrganizationId organizationId)
        : base(ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown,
            new ProblemParameter(ParameterName.OrganizationId, organizationId.ToString()))
    {
    }
}
