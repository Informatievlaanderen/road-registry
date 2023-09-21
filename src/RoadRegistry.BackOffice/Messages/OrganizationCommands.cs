namespace RoadRegistry.BackOffice.Messages;

using System;

public static class OrganizationCommands
{
    public static readonly Type[] All =
    {
        typeof(CreateOrganizationAccepted),
        typeof(DeleteOrganizationAccepted),
        typeof(RenameOrganizationAccepted),
        typeof(ChangeOrganizationAccepted)
    };
}
