namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenDeletingOrganization;

using Framework.Testing;
using NodaTime.Text;
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
    public Task ThenDeleteOrganizationRejected()
    {
        var command = new DeleteOrganization
        {
            Code = TestData.ChangedByOrganization
        };

        return Run(scenario => scenario
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new DeleteOrganizationRejected
            {
                Code = command.Code,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }
}
