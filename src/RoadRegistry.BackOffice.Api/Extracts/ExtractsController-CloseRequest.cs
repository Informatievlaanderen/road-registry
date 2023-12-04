namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string CloseRoute = "{downloadId}/close";

    [HttpPut(CloseRoute, Name = nameof(Close))]
    [SwaggerOperation(OperationId = nameof(Close), Description = "")]
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
                throw new ValidationException("OngeldigeReden", new[]
                {
                    new ValidationFailure(nameof(requestBody.Reason), "Opgegeven reden is niet geldig.")
                });
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
