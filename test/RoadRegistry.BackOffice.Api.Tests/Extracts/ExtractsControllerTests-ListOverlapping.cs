namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Api.Extracts;
using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task ListOverlapping_ReturnsExpectedResult()
    {
        //TODO-rik test, zeker special cases als de grens gelijk is
        _editorContext

        var result = await Controller.ListOverlaps(new ExtractsController.ListOverlappingParameters
            {
                Contour = "POLYGON ()"
            },
            new ExtractsController.ListOverlappingParametersValidator(),
            CancellationToken.None);

        //var jsonResult = Assert.IsType<OkObjectResult>(result);
        //Assert.IsType<FeatureCollection>(jsonResult.Value);
    }
}
