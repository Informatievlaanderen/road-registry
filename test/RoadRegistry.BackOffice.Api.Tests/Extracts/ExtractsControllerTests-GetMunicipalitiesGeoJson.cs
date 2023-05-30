namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task GetMunicipalitiesGeoJson_ReturnsExpectedResult()
    {
        var result = await Controller.GetMunicipalitiesGeoJson(CancellationToken.None);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.IsType<FeatureCollection>(jsonResult.Value);
    }
}
