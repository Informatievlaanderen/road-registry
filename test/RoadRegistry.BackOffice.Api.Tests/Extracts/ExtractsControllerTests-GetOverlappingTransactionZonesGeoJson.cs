namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using System.Threading;
using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task GetOverlappingTransactionZonesGeoJson_ReturnsExpectedResult()
    {
        var result = await Controller.GetOverlappingTransactionZonesGeoJson(CancellationToken.None);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.IsType<FeatureCollection>(jsonResult.Value);
    }
}
