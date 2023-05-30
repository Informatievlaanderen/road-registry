namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Extensions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;
using MultiLineString = GeoJSON.Net.Geometry.MultiLineString;

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
            if (!DownloadId.CanParse(downloadId)) throw new DownloadExtractNotFoundException();

            var request = new ExtractDetailsRequest(DownloadId.Parse(downloadId));
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new ExtractDetailsResponseBody
            {
                DownloadId = response.DownloadId.ToString(),
                Description = response.Description,
                Contour = response.Contour.ToGeoJson(),
                ExtractRequestId = response.ExtractRequestId,
                RequestOn = response.RequestedOn,
                UploadExpected = response.UploadExpected
            });
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (DownloadExtractNotFoundException)
        {
            return NotFound();
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }
}

internal record ExtractDetailsResponseBody()
{
    public string DownloadId { get; set; }
    public string Description { get; set; }
    public GeoJSON.Net.Geometry.MultiPolygon Contour { get; set; }
    public string ExtractRequestId { get; set; }
    public DateTimeOffset RequestOn { get; set; }
    public bool UploadExpected { get; set; }
}
