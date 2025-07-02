namespace RoadRegistry.BackOffice.Api.Uploads;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Jobs;

public partial class UploadController
{
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("jobs/{jobId:guid}/upload")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UploadJob(
        [FromRoute] Guid jobId,
        [FromForm(Name = "file")] IFormFile archive,
        CancellationToken cancellationToken,
        [FromServices] IHostEnvironment hostEnvironment)
    {
        // Only exists for local dev testing the entire upload flow (in UI)
        if (!hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }

        var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
        var request = new JobUploadArchiveRequest(jobId, requestArchive);

        await _mediator.Send(request, cancellationToken);

        return NoContent();
    }
}
