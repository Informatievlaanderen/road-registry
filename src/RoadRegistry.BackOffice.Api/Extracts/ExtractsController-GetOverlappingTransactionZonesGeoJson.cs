namespace RoadRegistry.BackOffice.Api.Extracts;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public partial class ExtractsController
{
    private const string GetOverlappingTransactionZonesGeoJsonRoute = "overlappingtransactionzones.geojson";

    /// <summary>
    ///     Gets the overlapping geometries of transaction zones in a GeoJson format.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>JsonResult.</returns>
    [HttpGet(GetOverlappingTransactionZonesGeoJsonRoute, Name = nameof(GetOverlappingTransactionZonesGeoJson))]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = nameof(GetOverlappingTransactionZonesGeoJson))]
    public async Task<JsonResult> GetOverlappingTransactionZonesGeoJson(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOverlappingTransactionZonesGeoJsonRequest(), cancellationToken);

        return new JsonResult(response.FeatureCollection);
    }
}
