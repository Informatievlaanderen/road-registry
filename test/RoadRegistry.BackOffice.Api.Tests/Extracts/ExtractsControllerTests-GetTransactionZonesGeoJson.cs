namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task GetTransactionZonesGeoJson_ReturnsExpectedResult()
    {
        var result = await Controller.GetTransactionZonesGeoJson(CancellationToken.None);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.IsType<FeatureCollection>(jsonResult.Value);
    }
}
