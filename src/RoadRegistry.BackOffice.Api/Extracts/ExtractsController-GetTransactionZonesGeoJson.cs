namespace RoadRegistry.BackOffice.Api.Extracts;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public partial class ExtractsController
{
    private const string GetTransactionZonesGeoJsonRoute = "transactionzones.geojson";

    /// <summary>
    ///     Gets the transaction zones in a GeoJson format.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>JsonResult.</returns>
    [HttpGet(GetTransactionZonesGeoJsonRoute, Name = nameof(GetTransactionZonesGeoJson))]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = nameof(GetTransactionZonesGeoJson))]
    public async Task<JsonResult> GetTransactionZonesGeoJson(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetTransactionZonesGeoJsonRequest(), cancellationToken);

        return new JsonResult(response.FeatureCollection);
    }
}
