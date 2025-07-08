namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;

public class ImportedOrganizationBuilder
{
    private readonly OrganizationId _organizationId;
    private readonly OrganizationName _organizationName;
    private string _when;

    private ImportedOrganizationBuilder(Fixture fixture)
    {
        _organizationId = fixture.Create<OrganizationId>();
        _organizationName = fixture.Create<OrganizationName>();
        WithClock(SystemClock.Instance);
    }

    public ImportedOrganizationBuilder(RoadNetworkTestData testData)
        : this(testData.ObjectProvider)
    {
        _organizationId = testData.ChangedByOrganization;
        _organizationName = testData.ChangedByOrganizationName;
    }

    public ImportedOrganizationBuilder WithClock(IClock clock)
    {
        return WithWhen(InstantPattern.ExtendedIso.Format(clock.GetCurrentInstant()));
    }
    public ImportedOrganizationBuilder WithWhen(string when)
    {
        _when = when;
        return this;
    }

    public ImportedOrganization Build()
    {
        return new ImportedOrganization
        {
            Code = _organizationId,
            Name = _organizationName,
            When = _when
        };
    }
}
