namespace RoadRegistry.BackOffice.Api.Information;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Editor.Schema;
using GeoJSON.Net.Feature;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;

public partial class InformationController
{
    /// <summary>
    ///     Gets the information.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>IActionResult.</returns>
    [HttpGet(Name = nameof(GetInformation))]
    [SwaggerOperation(OperationId = nameof(GetInformation), Description = "")]
    public async Task<IActionResult> GetInformation([FromServices] EditorContext context)
    {
        var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (info == null || !info.CompletedImport)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return new JsonResult(RoadNetworkInformationResponse.From(info));
    }

    [HttpGet("municipalities.geojson", Name = nameof(GetMunicipalities))]
    [SwaggerOperation(OperationId = nameof(GetMunicipalities), Description = "")]
    public async Task<IActionResult> GetMunicipalities([FromServices] EditorContext context)
    {
        var municipalities = await context.MunicipalityGeometries.ToListAsync(HttpContext.RequestAborted);

        return new JsonResult(new FeatureCollection(municipalities
            .Select(municipality => new Feature(((MultiPolygon)municipality.Geometry).ToGeoJson(), new
            {
                municipality.NisCode
            }))
            .ToList()
        ));
    }

    //[HttpGet("wms", Name = nameof(Wms))]
    //[SwaggerOperation(OperationId = nameof(Wms), Description = "")]
    //public async Task Wms()
    //{
    //    var client = new HttpClient();
    //    var url = "https://geo.api.beta-vlaanderen.be/Wegenregister/wms";
    //    var request = new HttpRequestMessage(HttpMethod.Get, $"{url}{HttpContext.Request.QueryString}");
    //    foreach (var header in HttpContext.Request.Headers.Where(x => x.Key != "Host"))
    //        foreach (var headerValue in header.Value)
    //        {
    //            request.Headers.Add(header.Key, headerValue);
    //        }

    //    var responseMessage = await client.SendAsync(request, HttpContext.RequestAborted);

    //    var response = HttpContext.Response;
    //    response.StatusCode = (int)responseMessage.StatusCode;

    //    response.Headers.Clear();
    //    foreach (var header in responseMessage.Headers)
    //    {
    //        response.Headers[header.Key] = header.Value.ToArray();
    //    }
    //    await responseMessage.Content.CopyToAsync(response.Body);
    //}
}

public class RoadNetworkInformationResponse
{
    /// <summary>
    ///     Gets or sets a value indicating whether [completed import].
    /// </summary>
    public bool CompletedImport { get; set; }

    /// <summary>
    ///     Gets or sets the grade separated junction count.
    /// </summary>
    /// <value>The grade separated junction count.</value>
    public int GradeSeparatedJunctionCount { get; set; }

    /// <summary>
    ///     Gets or sets the organization count.
    /// </summary>
    /// <value>The organization count.</value>
    public int OrganizationCount { get; set; }

    /// <summary>
    ///     Gets or sets the road node count.
    /// </summary>
    /// <value>The road node count.</value>
    public int RoadNodeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment count.
    /// </summary>
    /// <value>The road segment count.</value>
    public int RoadSegmentCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment european road attribute count.
    /// </summary>
    /// <value>The road segment european road attribute count.</value>
    public int RoadSegmentEuropeanRoadAttributeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment lane attribute count.
    /// </summary>
    /// <value>The road segment lane attribute count.</value>
    public int RoadSegmentLaneAttributeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment national road attribute count.
    /// </summary>
    /// <value>The road segment national road attribute count.</value>
    public int RoadSegmentNationalRoadAttributeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment numbered road attribute count.
    /// </summary>
    /// <value>The road segment numbered road attribute count.</value>
    public int RoadSegmentNumberedRoadAttributeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment surface attribute count.
    /// </summary>
    /// <value>The road segment surface attribute count.</value>
    public int RoadSegmentSurfaceAttributeCount { get; set; }

    /// <summary>
    ///     Gets or sets the road segment width attribute count.
    /// </summary>
    /// <value>The road segment width attribute count.</value>
    public int RoadSegmentWidthAttributeCount { get; set; }

    /// <summary>
    ///     Froms the specified information.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <returns>RoadNetworkInformationResponse.</returns>
    /// <exception cref="System.ArgumentNullException">info</exception>
    public static RoadNetworkInformationResponse From(RoadNetworkInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        return new RoadNetworkInformationResponse
        {
            CompletedImport = info.CompletedImport,
            OrganizationCount = info.OrganizationCount,
            RoadNodeCount = info.RoadNodeCount,
            RoadSegmentCount = info.RoadSegmentCount,
            RoadSegmentEuropeanRoadAttributeCount = info.RoadSegmentEuropeanRoadAttributeCount,
            RoadSegmentNationalRoadAttributeCount = info.RoadSegmentNationalRoadAttributeCount,
            RoadSegmentNumberedRoadAttributeCount = info.RoadSegmentNumberedRoadAttributeCount,
            RoadSegmentLaneAttributeCount = info.RoadSegmentLaneAttributeCount,
            RoadSegmentSurfaceAttributeCount = info.RoadSegmentSurfaceAttributeCount,
            RoadSegmentWidthAttributeCount = info.RoadSegmentWidthAttributeCount,
            GradeSeparatedJunctionCount = info.GradeSeparatedJunctionCount
        };
    }
}
