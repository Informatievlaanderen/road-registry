namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Requests the download by nis code.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(DownloadExtractResponseBody), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByNisCode))]
    [HttpPost("downloadrequests/byniscode", Name = nameof(RequestDownloadByNisCode))]
    public async Task<ActionResult> RequestDownloadByNisCode([FromBody] DownloadExtractByNisCodeRequestBody body, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByNisCodeRequest(body.NisCode, body.Buffer, body.Description, body.IsInformative);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
        }
        catch (DownloadExtractByNisCodeNotFoundException)
        {
            return NotFound();
        }
    }
}

public record DownloadExtractByNisCodeRequestBody(int Buffer, string Description, string NisCode, bool IsInformative);
