namespace RoadRegistry.BackOffice.Api.Extracts;

using Editor.Schema;
using Extensions;
using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    private const string GetTransactionZonesGeoJsonRoute = "transactionzones.geojson";

    /// <summary>
    ///     Gets the transaction zones in a GeoJson format.
    /// </summary>
    /// <param name="context">The EditorContext.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetTransactionZonesGeoJsonRoute, Name = nameof(GetTransactionZonesGeoJson))]
    [SwaggerOperation(OperationId = nameof(GetTransactionZonesGeoJson), Description = "")]
    public async Task<IActionResult> GetTransactionZonesGeoJson([FromServices] EditorContext context, CancellationToken cancellationToken)
    {
        var transactionZones = await context.ExtractRequests
            .Where(x => x.UploadExpected == true)
            .ToListAsync(cancellationToken);

        return new JsonResult(new FeatureCollection(transactionZones
            .Select(municipality => new Feature(((MultiPolygon)municipality.Contour).ToGeoJson(), new
            {
                municipality.Description,
                municipality.DownloadId,
                municipality.ExternalRequestId,
                municipality.RequestedOn
            }))
            .ToList()
        ));
    }
}
