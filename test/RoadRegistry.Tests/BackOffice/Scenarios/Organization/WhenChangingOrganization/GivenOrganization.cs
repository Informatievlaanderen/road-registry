namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenChangingOrganization;

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
    public Task ThenChangeOrganizationAccepted()
    {
        var command = new ChangeOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = "abc_changed",
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>(),
            IsMaintainer = true,
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
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new ChangeOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                NameModified = true,
                OvoCode = command.OvoCode,
                OvoCodeModified = true,
                KboNumber = command.KboNumber,
                KboNumberModified = true,
                IsMaintainer = true,
                IsMaintainerModified = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public async Task StateCheck()
    {
        var command = new ChangeOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = "abc_changed",
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>(),
            IsMaintainer = true,
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
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new ChangeOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                NameModified = true,
                OvoCode = command.OvoCode,
                OvoCodeModified = true,
                KboNumber = command.KboNumber,
                KboNumberModified = true,
                IsMaintainer = true,
                IsMaintainerModified = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        var organization = await RoadRegistryContext.Organizations.FindAsync(TestData.ChangedByOrganization);

        organization.Translation.Identifier.ToString().Should().Be(command.Code);
        organization.Translation.Name.ToString().Should().Be(command.Name);
        organization.OvoCode.ToString().Should().Be(command.OvoCode);
        organization.KboNumber.ToString().Should().Be(command.KboNumber);
        organization.IsMaintainer.Should().Be(command.IsMaintainer!.Value);
    }
}
