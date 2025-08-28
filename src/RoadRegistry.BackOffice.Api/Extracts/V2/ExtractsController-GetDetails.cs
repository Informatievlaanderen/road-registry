namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts.V2;
using Exceptions;
using Extensions;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{    /// <summary>
    ///     Gets the extract details (v2).
    /// </summary>
    [ProducesResponseType(typeof(ExtractDetailsResponseBody), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation]
    [HttpGet("{downloadId}", Name = nameof(GetDetails))]
    public async Task<ActionResult> GetDetails(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
            {
                throw new InvalidGuidValidationException("DownloadId");
            }

            var request = new ExtractDetailsRequest(parsedDownloadId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new ExtractDetailsResponseBody
            {
                DownloadId = response.DownloadId,
                Description = response.Description,
                Contour = response.Contour.ToGeoJson(),
                ExtractRequestId = response.ExtractRequestId,
                RequestedOn = response.RequestedOn,
                IsInformative = response.IsInformative,
                DownloadStatus = response.DownloadStatus,
                DownloadedOn = response.DownloadedOn,
                UploadStatus = response.UploadStatus,
                UploadId = response.UploadId,
                TicketId = response.TicketId,
                Closed = response.Closed,
            });
        }
        catch (ExtractRequestNotFoundException)
        {
            return NotFound();
        }
    }
}

public record ExtractDetailsResponseBody
{
    public string DownloadId { get; init; }
    public string Description { get; init; }
    public MultiPolygon Contour { get; init; }
    public string ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; init; }
    public bool IsInformative { get; init; }
    public string DownloadStatus { get; init; }
    public DateTimeOffset? DownloadedOn { get; init; }
    public bool Closed { get; init; }
    public string? UploadStatus { get; init; }
    public string? UploadId { get; init; }
    public string? TicketId { get; init; }
}
