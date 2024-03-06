namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;

public partial class UploadController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip te uploaden.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    //TODO-rik docs en responsetype
    [ProducesResponseType(typeof(FileCallbackResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FileCallbackResultExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetUploadPreSignedUrl), Description = "")]
    [HttpPost("jobs", Name = nameof(GetUploadPreSignedUrl))]
    public async Task<IActionResult> GetUploadPreSignedUrl(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new UploadPreSignedUrlRequest(), cancellationToken));
    }
}
