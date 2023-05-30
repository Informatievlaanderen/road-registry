namespace RoadRegistry.BackOffice.Api.Extracts;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    private const string GetMunicipalitiesGeoJsonRoute = "municipalities.geojson";

    /// <summary>
    ///     Gets the municipalities in a GeoJson format.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>JsonResult.</returns>
    [HttpGet(GetMunicipalitiesGeoJsonRoute, Name = nameof(GetMunicipalitiesGeoJson))]
    [SwaggerOperation(OperationId = nameof(GetMunicipalitiesGeoJson))]
    public async Task<JsonResult> GetMunicipalitiesGeoJson(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMunicipalitiesGeoJsonRequest(), cancellationToken);
        
        return new JsonResult(response.FeatureCollection);
    }
}
