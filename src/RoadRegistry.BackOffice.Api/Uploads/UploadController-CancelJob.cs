namespace RoadRegistry.BackOffice.Api.Uploads;
using Jobs.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class UploadController
{
    [HttpDelete("jobs/{jobId:guid}")]
    public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelJobRequest(jobId), cancellationToken);
        return NoContent();
    }
}
