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
                UploadId = response.UploadId,
                UploadStatus = response.UploadStatus,
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
    public string DownloadId { get; set; }
    public string Description { get; set; }
    public MultiPolygon Contour { get; set; }
    public string ExtractRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; set; }
    public string DownloadStatus { get; set; }
    public bool Closed { get; set; }
    public string? UploadId { get; set; }
    public string UploadStatus { get; set; }
    public string? TicketId { get; set; }
}
