namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;

public static partial class ScenarioExtensions
{
    public static IScenarioGivenStateBuilder GivenOrganization(
        this IScenarioGivenStateBuilder builder,
        MaintenanceAuthority maintainanceAuthority)
    {
        return builder.Given(Organizations.ToStreamName(new OrganizationId(maintainanceAuthority.Code)), new ImportedOrganization
        {
            Code = maintainanceAuthority.Code,
            Name = maintainanceAuthority.Name
        });
    }
}
