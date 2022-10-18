namespace RoadRegistry.BackOffice.Api.Information;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Information")]
[ApiKeyAuth("Road")]
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