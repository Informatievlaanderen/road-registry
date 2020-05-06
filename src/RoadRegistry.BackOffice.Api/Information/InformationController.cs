namespace RoadRegistry.BackOffice.Api.Information
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Editor.Schema;
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
            return new JsonResult(RoadNetworkInformationResponse.From(info));
        }
    }
}
