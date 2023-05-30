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
    private const string GetMunicipalitiesGeoJsonRoute = "municipalities.geojson";

    /// <summary>
    ///     Gets the municipalities in a GeoJson format.
    /// </summary>
    /// <param name="context">The EditorContext.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetMunicipalitiesGeoJsonRoute, Name = nameof(GetMunicipalitiesGeoJson))]
    [SwaggerOperation(OperationId = nameof(GetMunicipalitiesGeoJson), Description = "")]
    public async Task<IActionResult> GetMunicipalitiesGeoJson([FromServices] EditorContext context, CancellationToken cancellationToken)
    {
        var municipalities = await context.MunicipalityGeometries.ToListAsync(cancellationToken);

        return new JsonResult(new FeatureCollection(municipalities
            .Select(municipality => new Feature(((MultiPolygon)municipality.Geometry).ToGeoJson(), new
            {
                municipality.NisCode
            }))
            .ToList()
        ));
    }
}
