namespace RoadRegistry.Tests.BackOffice.Scenarios.Organization.WhenCreatingOrganization;

using AutoFixture;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Tests.Framework.Testing;
using Command = RoadRegistry.BackOffice.Framework.Command;

public class GivenOrganization : RoadNetworkTestBase
{
    public GivenOrganization(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task ThenCreateOrganizationRejected()
    {
        var command = new CreateOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = ObjectProvider.Create<OrganizationName>(),
            OvoCode = ObjectProvider.Create<OrganizationOvoCode>(),
            KboNumber = ObjectProvider.Create<OrganizationKboNumber>()
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
            .Then(Organizations.ToStreamName(TestData.ChangedByOrganization), new CreateOrganizationRejected
            {
                Code = command.Code,
                Name = command.Name,
                OvoCode = command.OvoCode,
                KboNumber = command.KboNumber,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

}
