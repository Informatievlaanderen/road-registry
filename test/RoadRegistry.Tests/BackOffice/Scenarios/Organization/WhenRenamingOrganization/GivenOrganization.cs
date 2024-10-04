namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenRenamingOrganization;

using AutoFixture;
using FluentAssertions;
using Framework.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice;
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
    public Task ThenRenameOrganizationAccepted()
    {
        var command = new RenameOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>()
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
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new RenameOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public async Task StateCheck()
    {
        var command = new RenameOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>()
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
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new RenameOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        var organization = await Organizations.FindAsync(TestData.ChangedByOrganization);

        organization.Translation.Identifier.ToString().Should().Be(command.Code);
        organization.Translation.Name.ToString().Should().Be(command.Name);
    }
}
