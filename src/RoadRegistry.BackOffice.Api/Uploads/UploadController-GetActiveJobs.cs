namespace RoadRegistry.BackOffice.Api.Uploads;
using Jobs.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

public partial class UploadController
{
    [HttpGet("jobs/active")]
    public async Task<IActionResult> GetActiveJobs(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetActiveJobsRequest(), cancellationToken));
    }
}
