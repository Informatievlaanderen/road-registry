namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenCreatingOrganization;

using AutoFixture;
using FluentAssertions;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Tests.Framework.Testing;
using Command = RoadRegistry.BackOffice.Framework.Command;

public class GivenNoOrganization : RoadNetworkTestBase
{
    public GivenNoOrganization(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ThenCreateOrganizationAccepted()
    {
        var command = new CreateOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>(),
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>()
        };

        return Run(scenario => scenario
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new CreateOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                OvoCode = command.OvoCode,
                KboNumber = command.KboNumber,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public async Task StateCheck()
    {
        var command = new CreateOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>(),
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>()
        };

        await Run(scenario => scenario
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new CreateOrganizationAccepted
            {
                Code = command.Code,
                Name = command.Name,
                OvoCode = command.OvoCode,
                KboNumber = command.KboNumber,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        var organization = await RoadRegistryContext.Organizations.FindAsync(TestData.ChangedByOrganization);

        organization.Translation.Identifier.ToString().Should().Be(command.Code);
        organization.Translation.Name.ToString().Should().Be(command.Name);
        organization.OvoCode.ToString().Should().Be(command.OvoCode);
        organization.KboNumber.ToString().Should().Be(command.KboNumber);
        organization.IsMaintainer.Should().BeFalse();
    }
}
