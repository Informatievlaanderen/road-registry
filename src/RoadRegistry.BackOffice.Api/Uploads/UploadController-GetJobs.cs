namespace RoadRegistry.BackOffice.Api.Uploads;

using System;
using Abstractions;
using Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Jobs;

public partial class UploadController
{
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs([FromQuery] string statuses, CancellationToken cancellationToken)
    {
        var offset = Convert.ToInt32(HttpContext.Request.Query["offset"]);
        var limit = Convert.ToInt32(HttpContext.Request.Query["limit"]);
        var pagination = new Pagination(offset, limit);
        var statusesFilter = new EnumFilter<JobStatus>(HttpContext.Request.Query, nameof(statuses));

        return Ok(await _mediator.Send(new GetJobsRequest(pagination, statusesFilter), cancellationToken));
    }
}
