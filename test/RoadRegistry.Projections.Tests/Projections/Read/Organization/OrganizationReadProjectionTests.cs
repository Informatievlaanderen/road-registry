namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.Organization;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;

public class OrganizationReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(new OrganizationReadProjection());

    [Fact]
    public async Task WhenOrganizationWasImported_ThenStored()
    {
        var scenario = Scenario();
        var organizationId = new OrganizationId("TestOrg");

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = organizationId,
            Name = "Test Organization",
            Provenance = Provenance
        });

        var org = await scenario.Load<OrganizationReadItem>(organizationId.ToString());
        Assert.NotNull(org);
        Assert.Equal("Test Organization", org!.Name);
        Assert.False(org.IsMaintainer);
        Assert.False(org.IsRemoved);
    }

    [Fact]
    public async Task WhenOrganizationWasCreated_ThenStored()
    {
        var scenario = Scenario();
        var organizationId = new OrganizationId("TestOrg");

        await scenario.GivenAsync(new OrganizationWasCreated
        {
            OrganizationId = organizationId,
            Name = "Test Organization",
            OvoCode = "OVO123456",
            KboNumber = null,
            Provenance = Provenance
        });

        var org = await scenario.Load<OrganizationReadItem>(organizationId.ToString());
        Assert.NotNull(org);
        Assert.Equal("Test Organization", org!.Name);
        Assert.Equal("OVO123456", org.OvoCode);
        Assert.Null(org.KboNumber);
        Assert.False(org.IsRemoved);
    }

    [Fact]
    public async Task WhenOrganizationWasModified_ThenUpdated()
    {
        var scenario = Scenario();
        var organizationId = new OrganizationId("TestOrg");

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = organizationId,
            Name = "Original Name",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            Name = "Updated Name",
            IsMaintainer = true,
            Provenance = Provenance
        });

        var org = await scenario.Load<OrganizationReadItem>(organizationId.ToString());
        Assert.Equal("Updated Name", org!.Name);
        Assert.True(org.IsMaintainer);
    }

    [Fact]
    public async Task WhenOrganizationWasModified_WithNullFields_ThenKeepsExistingValues()
    {
        var scenario = Scenario();
        var organizationId = new OrganizationId("TestOrg");

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = organizationId,
            Name = "Original Name",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            Name = null,
            Provenance = Provenance
        });

        var org = await scenario.Load<OrganizationReadItem>(organizationId.ToString());
        Assert.Equal("Original Name", org!.Name);
    }

    [Fact]
    public async Task WhenOrganizationWasRemoved_ThenMarkedRemoved()
    {
        var scenario = Scenario();
        var organizationId = new OrganizationId("TestOrg");

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = organizationId,
            Name = "Test Organization",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new OrganizationWasRemoved
        {
            OrganizationId = organizationId,
            Provenance = Provenance
        });

        var org = await scenario.Load<OrganizationReadItem>(organizationId.ToString());
        Assert.True(org!.IsRemoved);
    }
}
