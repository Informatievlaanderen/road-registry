namespace RoadRegistry.BackOffice.Api.Tests.Organizations;

using System.Linq;
using Api.Organizations;
using Microsoft.AspNetCore.Mvc;

public partial class OrganizationControllerTests
{
    [Fact]
    public async Task WhenDownloadingOrganizations_ThenShouldOnlyGetMaintainers()
    {
        var result = await Controller.Get(CancellationToken.None);

        var organizations = (GetOrganizationsResponse)Assert.IsType<OkObjectResult>(result).Value!;

        Assert.Equal(2, organizations.Count);
        Assert.NotEqual(organizations.Count, _editorContext.OrganizationsV2.Count());

        Assert.Equal("AGIV", organizations[0].Code);
        Assert.Equal("Agentschap voor Geografische Informatie Vlaanderen", organizations[0].Label);
        Assert.Null(organizations[0].OvoCode);

        Assert.Equal("11040", organizations[1].Code);
        Assert.Equal("Gemeente Schoten", organizations[1].Label);
        Assert.Equal("OVO002229", organizations[1].OvoCode);
    }
}
