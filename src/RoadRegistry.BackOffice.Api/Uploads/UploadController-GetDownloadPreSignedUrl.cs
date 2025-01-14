namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class UploadController
{
    private const string GetDownloadPreSignedUrlRoute = "{identifier}/presignedurl";

    /// <summary>
    ///     Gets the pre-signed url to download the uploaded extract.
    /// </summary>
    /// <param name="identifier">De identificator van de upload.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de upload gevonden is.</response>
    /// <response code="404">Als de upload niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetDownloadPreSignedUrlRoute, Name = nameof(GetDownloadPreSignedUrlForUpload))]
    [SwaggerOperation(OperationId = nameof(GetDownloadPreSignedUrlForUpload), Description = "")]
    public async Task<IActionResult> GetDownloadPreSignedUrlForUpload(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetUploadFilePreSignedUrlRequest(identifier);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new GetDownloadPreSignedUrlResponse
            {
                DownloadUrl = response.PreSignedUrl,
                FileName = response.FileName
            });
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }

    public class GetDownloadPreSignedUrlResponse
    {
        public string DownloadUrl { get; init; }
        public string FileName { get; init; }
    }
}
