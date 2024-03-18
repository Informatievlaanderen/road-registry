namespace RoadRegistry.BackOffice.Api.Uploads;
using Jobs.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class UploadController
{
    [HttpGet("jobs/{jobId:guid}")]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetJobByIdRequest(jobId), cancellationToken));
    }
}
