namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(typeof(ExtractDetailsResponseBody), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetDetails))]
    [HttpGet("{downloadId}", Name = nameof(GetDetails))]
    public async Task<ActionResult> GetDetails(
        [FromRoute] string downloadId,
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
            {
                throw new InvalidGuidValidationException("DownloadId");
            }

            var request = new ExtractDetailsRequest(parsedDownloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new ExtractDetailsResponseBody
            {
                DownloadId = response.DownloadId,
                Description = response.Description,
                Contour = response.Contour.ToGeoJson(),
                ExtractRequestId = response.ExtractRequestId,
                RequestedOn = response.RequestedOn,
                IsInformative = response.IsInformative,
                ArchiveId = response.ArchiveId,
                TicketId = response.TicketId,
                DownloadAvailable = response.DownloadAvailable,
                ExtractDownloadTimeoutOccurred = response.ExtractDownloadTimeoutOccurred,
            });
        }
        catch (ExtractArchiveNotCreatedException)
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (DownloadExtractNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
        catch (ExtractDownloadNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
    }
}

public record ExtractDetailsResponseBody
{
    public string DownloadId { get; set; }
    public string Description { get; set; }
    public GeoJSON.Net.Geometry.MultiPolygon Contour { get; set; }
    public string ExtractRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; set; }
    public bool DownloadAvailable { get; set; }
    public bool ExtractDownloadTimeoutOccurred { get; set; }
    public string? ArchiveId { get; set; }
    public string? TicketId { get; set; }
}
