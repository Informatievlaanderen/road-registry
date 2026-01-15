namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Threading;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     Gets the extract details (v2).
    /// </summary>
    [ProducesResponseType(typeof(ExtractDetailsResponseBody), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation]
    [HttpGet("{downloadId}", Name = nameof(GetExtractDetails))]
    public async Task<ActionResult> GetExtractDetails(
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
                Beschrijving = response.Description,
                ExterneId = response.ExternalExtractRequestId,
                Contour = response.Contour.ToGeoJson(),
                AangevraagdOp = response.RequestedOn,
                Informatief = response.IsInformative,
                DownloadStatus = response.DownloadStatus,
                GedownloadOp = response.DownloadedOn,
                UploadStatus = response.UploadStatus,
                UploadId = response.UploadId,
                TicketId = response.TicketId,
                Gesloten = response.Closed,
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
    public string Beschrijving { get; init; }
    public string? ExterneId { get; init; }
    public MultiPolygon Contour { get; init; }
    public DateTimeOffset AangevraagdOp { get; init; }
    public bool Informatief { get; init; }
    public string DownloadStatus { get; init; }
    public DateTimeOffset? GedownloadOp { get; init; }
    public string? UploadStatus { get; init; }
    public string? UploadId { get; init; }
    public string? TicketId { get; init; }
    public bool Gesloten { get; init; }
}
