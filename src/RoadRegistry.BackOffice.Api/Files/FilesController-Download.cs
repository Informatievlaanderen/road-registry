namespace RoadRegistry.BackOffice.Api.Files;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Files;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

public partial class FilesController
{
    [HttpGet("download")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Download(
        [FromQuery] string bucket,
        [FromQuery] string blob,
        [FromQuery] string fileName,
        CancellationToken cancellationToken,
        [FromServices] IHostEnvironment hostEnvironment)
    {
        // Only exists for local dev testing the entire flow (in UI)
        if (!hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }

        var request = new DownloadFileRequest(bucket, new BlobName(blob), fileName);

        var response = await _mediator.Send(request, cancellationToken);

        return new FileCallbackResult(response);
    }
}
