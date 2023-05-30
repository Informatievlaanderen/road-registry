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
    private const string GetOverlappingTransactionZonesGeoJsonRoute = "overlappingtransactionzones.geojson";

    /// <summary>
    ///     Gets the overlapping geometries of transaction zones in a GeoJson format.
    /// </summary>
    /// <param name="context">The EditorContext.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetOverlappingTransactionZonesGeoJsonRoute, Name = nameof(GetOverlappingTransactionZonesGeoJson))]
    [SwaggerOperation(OperationId = nameof(GetOverlappingTransactionZonesGeoJson), Description = "")]
    public async Task<IActionResult> GetOverlappingTransactionZonesGeoJson([FromServices] EditorContext context, CancellationToken cancellationToken)
    {
        var transactionZones = await context.ExtractRequests
            .Where(x => x.UploadExpected == true)
            .ToListAsync(cancellationToken);

        var intersectionGeometries = (
            from t1 in transactionZones
            from t2 in transactionZones
            where t1.DownloadId != t2.DownloadId
            let intersection = t1.Contour.Intersection(t2.Contour)
            where intersection is not null && intersection != Polygon.Empty
            select intersection
        ).Distinct().ToArray();
        
        return new JsonResult(new FeatureCollection(intersectionGeometries
            .Select(geometry => new Feature(geometry is MultiPolygon multiPolygon ? multiPolygon.ToGeoJson() : ((Polygon)geometry).ToGeoJson()))
            .ToList()
        ));
    }
}
