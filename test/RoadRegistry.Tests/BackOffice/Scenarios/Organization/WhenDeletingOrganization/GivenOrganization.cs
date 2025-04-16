namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenDeletingOrganization;

using FluentAssertions;
using Framework.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Command = RoadRegistry.BackOffice.Framework.Command;

public class GivenOrganization : RoadNetworkTestBase
{
    public GivenOrganization(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ThenDeleteOrganizationAccepted()
    {
        var command = new DeleteOrganization
        {
            Code = TestData.ChangedByOrganization
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new DeleteOrganizationAccepted
            {
                Code = command.Code,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public async Task StateCheck()
    {
        var command = new DeleteOrganization
        {
            Code = TestData.ChangedByOrganization
        };

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new DeleteOrganizationAccepted
            {
                Code = command.Code,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        var organization = await RoadRegistryContext.Organizations.FindAsync(TestData.ChangedByOrganization);

        organization.Should().BeNull();
    }
}
