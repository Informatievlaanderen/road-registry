namespace RoadRegistry.BackOffice.Api.Information;

using Editor.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public partial class InformationController
{
    /// <summary>
    ///     Gets the information.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>IActionResult.</returns>
    [HttpGet(Name = nameof(GetInformation))]
    [AllowAnonymous]
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
