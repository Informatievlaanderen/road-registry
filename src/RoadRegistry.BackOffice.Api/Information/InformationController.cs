namespace RoadRegistry.BackOffice.Api.Information
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Schema;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("information")]
    [ApiExplorerSettings(GroupName = "Information")]
    public class InformationController : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> Get([FromServices] BackOfficeContext context)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
            return new JsonResult(RoadNetworkInformationResponse.From(info));
        }
    }
}
