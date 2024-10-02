namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenRenamingOrganization;

using AutoFixture;
using Framework.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Command = RoadRegistry.BackOffice.Framework.Command;

public class GivenNoOrganization : RoadNetworkTestBase
{
    public GivenNoOrganization(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ThenRenameOrganizationRejected()
    {
        var command = new RenameOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>()
        };

        return Run(scenario => scenario
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new RenameOrganizationRejected
            {
                Code = command.Code,
                Name = command.Name,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }
}
