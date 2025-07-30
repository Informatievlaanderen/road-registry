namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Messages;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(Close))]
    [HttpPost("{downloadId}/close", Name = nameof(Close))]
    public async Task<IActionResult> Close(
        [FromRoute] string downloadId,
        [FromBody] CloseRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
            {
                throw new InvalidGuidValidationException("DownloadId");
            }

            if (!Enum.TryParse<RoadNetworkExtractCloseReason>(requestBody.Reason, true, out var reason))
            {
                throw new ValidationException("OngeldigeReden", [
                    new ValidationFailure(nameof(requestBody.Reason), "Opgegeven reden is niet geldig.")
                ]);
            }

            var request = new CloseRoadNetworkExtractRequest(parsedDownloadId, reason);
            await _mediator.Send(request, cancellationToken);

            return Accepted();
        }
        catch (UploadExtractNotFoundException)
        {
            return NotFound();
        }
    }
}

public sealed record CloseRequestBody
{
    public string Reason { get; set; }
}
