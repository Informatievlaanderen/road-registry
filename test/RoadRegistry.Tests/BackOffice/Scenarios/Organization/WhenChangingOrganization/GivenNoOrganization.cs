namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenChangingOrganization;

using AutoFixture;
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
    public Task ThenChangeOrganizationRejected()
    {
        var command = new ChangeOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>(),
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>(),
            IsMaintainer = true,
        };

        return Run(scenario => scenario
            .When(new Command(command))
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new ChangeOrganizationRejected
            {
                Code = command.Code,
                Name = command.Name,
                OvoCode = command.OvoCode,
                KboNumber = command.KboNumber,
                IsMaintainer = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }
}
