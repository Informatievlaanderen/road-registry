namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadNodes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    [HttpPost("correct/roadnodeversions")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostCorrectRoadNodeVersions()
    {
        var response = await Mediator.Send(new CorrectRoadNodeVersionsRequest(), HttpContext.RequestAborted);
        return Accepted(response);
    }
}
