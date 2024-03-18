namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions;
using Infrastructure.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Jobs;

public partial class UploadController
{
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs([FromQuery] string statuses, CancellationToken cancellationToken)
    {
        var pagination = new Pagination(HttpContext.Request.Query);
        var statusesFilter = new EnumFilter<JobStatus>(HttpContext.Request.Query, nameof(statuses));

        return Ok(await _mediator.Send(new GetJobsRequest(pagination, statusesFilter), cancellationToken));
    }
}
