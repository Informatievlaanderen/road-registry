namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class UploadController
{
    private const string GetUploadRoute = "{identifier}";

    /// <summary>
    ///     Download een aangevraagd extract
    /// </summary>
    /// <param name="id">De identificator van het wegsegment.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetUploadRoute, Name = nameof(GetUpload))]
    [ProducesResponseType(typeof(FileCallbackResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FileCallbackResultExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ExtractDownloadNotFoundException))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetUpload), Description = "")]
    public async Task<IActionResult> GetUpload(string id, CancellationToken cancellationToken)
    {
        try
        {
            DownloadExtractRequest request = new(id);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }
}