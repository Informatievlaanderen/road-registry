namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;
using Editor.Schema.RoadNetworkChanges;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public partial class InformationControllerTests
{
    [Fact]
    public async Task When_requesting_roadnetwork_information()
    {
        var database = await ApplyChangeCollectionIntoContext(_fixture, archiveId => new RoadNetworkChange[] { });

        await using var editorContext = await _fixture.CreateEditorContextAsync(database);
        var result = await Controller.GetInformation(editorContext);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var response = Assert.IsType<RoadNetworkInformationResponse>(jsonResult.Value);

        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
    }
}