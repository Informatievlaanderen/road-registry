namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    [HttpPost("correct/roadsegmentversions")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostCorrectRoadSegmentVersions()
    {
        var response = await Mediator.Send(new CorrectRoadSegmentVersionsRequest(), HttpContext.RequestAborted);
        return Accepted(response);
    }
}
