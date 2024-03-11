namespace RoadRegistry.BackOffice.Api.Extracts;

using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.Jobs.Abstractions;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(UploadPreSignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FileCallbackResultExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetUploadPreSignedUrlForDownload), Description = "")]
    [HttpPost("download/{downloadId}/jobs", Name = nameof(GetUploadPreSignedUrlForDownload))]
    public async Task<IActionResult> GetUploadPreSignedUrlForDownload([FromRoute] string downloadId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new UploadPreSignedUrlRequest(UploadType.Extracts, downloadId), cancellationToken));
    }
}
