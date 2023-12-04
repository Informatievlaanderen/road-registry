namespace RoadRegistry.BackOffice.Api.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    private const string GetDetailsRoute = "{downloadId}";

    [HttpGet(GetDetailsRoute, Name = nameof(GetDetails))]
    [SwaggerOperation(OperationId = nameof(GetDetails), Description = "")]
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
                IsInformative = response.IsInformative
            });
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

internal record ExtractDetailsResponseBody
{
    public string DownloadId { get; set; }
    public string Description { get; set; }
    public GeoJSON.Net.Geometry.MultiPolygon Contour { get; set; }
    public string ExtractRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; set; }
}
