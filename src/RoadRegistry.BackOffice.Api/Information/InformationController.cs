namespace RoadRegistry.BackOffice.Api.Information;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiVersion("1.0")]
[AdvertiseApiVersions("1.0")]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Information")]
public class InformationController : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Get([FromServices] EditorContext context)
    {
        var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (info == null || !info.CompletedImport) return StatusCode(StatusCodes.Status503ServiceUnavailable);
        return new JsonResult(RoadNetworkInformationResponse.From(info));
    }
}
