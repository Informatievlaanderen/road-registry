namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string GetDetailsRoute = "{downloadId}";

    [HttpGet(GetDetailsRoute, Name = nameof(GetDetails))]
    [SwaggerOperation(OperationId = nameof(GetDetails), Description = "")]
    public async Task<ActionResult> GetDetails(
        [FromRoute] string downloadId,
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.CanParse(downloadId)) throw new DownloadExtractNotFoundException("");

            var request = new ExtractDetailsRequest(DownloadId.Parse(downloadId));
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(new ExtractDetailsResponseBody());
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (DownloadExtractNotFoundException)
        {
            return NotFound();
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }
}

internal record ExtractDetailsResponseBody()
{

}
