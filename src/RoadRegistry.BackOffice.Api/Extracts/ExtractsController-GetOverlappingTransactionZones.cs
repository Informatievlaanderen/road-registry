namespace RoadRegistry.BackOffice.Api.Extracts;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public partial class ExtractsController
{
    private const string GetOverlappingTransactionZonesRoute = "overlappingtransactionzones/list";

    /// <summary>
    ///     Gets the overlapping transaction zones.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>JsonResult.</returns>
    [HttpPost(GetOverlappingTransactionZonesRoute, Name = nameof(GetOverlappingTransactionZones))]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = nameof(GetOverlappingTransactionZones))]
    public async Task<JsonResult> GetOverlappingTransactionZones(GetOverlappingTransactionZonesParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOverlappingTransactionZonesRequest(parameters?.NisCode, parameters.Contour), cancellationToken);

        return new JsonResult(response.FeatureCollection);
    }

    public class GetOverlappingTransactionZonesParameters
    {
        public string NisCode { get; set; }
        public string Contour { get; set; }
    }
}
