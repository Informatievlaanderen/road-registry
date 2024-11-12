namespace RoadRegistry.BackOffice.Messages;

using System;

public static class OrganizationCommands
{
    public static readonly Type[] All =
    {
        typeof(CreateOrganization),
        typeof(DeleteOrganization),
        typeof(RenameOrganization),
        typeof(ChangeOrganization)
    };
}
