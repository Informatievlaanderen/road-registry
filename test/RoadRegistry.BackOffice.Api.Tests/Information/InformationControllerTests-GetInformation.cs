namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public partial class InformationControllerTests
{
    [Fact]
    public async Task When_requesting_roadnetwork_information()
    {
        _editorContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = true
        });
        await _editorContext.SaveChangesAsync();

        var result = await Controller.GetInformation(_editorContext);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.IsType<RoadNetworkInformationResponse>(jsonResult.Value);
    }
}
