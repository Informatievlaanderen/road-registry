namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class MaintenanceAuthorityCodeNotValid : Error
{
    public static class ParameterName
    {
        public const string OrganizationId = "OrganizationId";
    }

    public MaintenanceAuthorityCodeNotValid(OrganizationId organizationId)
        : base(ProblemCode.RoadSegment.MaintenanceAuthorityCode.NotValid,
            new ProblemParameter(ParameterName.OrganizationId, organizationId.ToString()))
    {
    }
}
