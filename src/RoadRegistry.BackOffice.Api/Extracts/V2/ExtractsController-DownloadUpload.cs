namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Gets the pre-signed url to download the uploaded extract.
    /// </summary>
    /// <param name="identifier">De identificator van de upload.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de upload gevonden is.</response>
    /// <response code="404">Als de upload niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(GetUploadDownloadPreSignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(DownloadUpload))]
    [HttpGet("{downloadId}/upload", Name = nameof(DownloadUpload))]
    public async Task<IActionResult> DownloadUpload(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        try
        {
            var request = new GetUploadFilePreSignedUrlRequest(parsedDownloadId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new GetUploadDownloadPreSignedUrlResponse
            {
                DownloadUrl = response.PreSignedUrl,
                FileName = response.FileName
            });
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }

    public class GetUploadDownloadPreSignedUrlResponse
    {
        public string DownloadUrl { get; init; }
        public string FileName { get; init; }
    }
}
