namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Extracts;
using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;

public partial class UploadController
{
    private const string PostUploadBeforeFeatureCompareRoute = "fc";

    /// <summary>
    ///     Posts the upload before feature compare.
    /// </summary>
    /// <param name="useFeatureCompareToggle"></param>
    /// <param name="useZipArchiveFeatureCompareTranslatorFeatureToggle"></param>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het bestand goed ontvangen werd.</response>
    /// <response code="404">Als het extract niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(PostUploadBeforeFeatureCompareRoute, Name = nameof(UploadBeforeFeatureCompare))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ExtractDownloadNotFoundException))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(UploadBeforeFeatureCompare), Description = "")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> UploadBeforeFeatureCompare(
        [FromServices] UseFeatureCompareFeatureToggle useFeatureCompareToggle,
        [FromServices] UseZipArchiveFeatureCompareTranslatorFeatureToggle useZipArchiveFeatureCompareTranslatorFeatureToggle,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        if (!useFeatureCompareToggle.FeatureEnabled)
        {
            return NotFound();
        }

        return await PostUpload(archive, async () =>
        {
            if (GetFeatureToggleValue(useZipArchiveFeatureCompareTranslatorFeatureToggle))
            {
                UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
                var request = new UploadExtractRequest(requestArchive)
                {
                    UseZipArchiveFeatureCompareTranslator = true
                };
                var response = await _mediator.Send(request, cancellationToken);
                return Accepted(new UploadExtractFeatureCompareResponseBody(response.ArchiveId.ToString()));
            }
            else
            {
                UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
                var request = new UploadExtractFeatureCompareRequest(archive.FileName, requestArchive);
                var response = await _mediator.Send(request, cancellationToken);
                return Accepted(new UploadExtractFeatureCompareResponseBody(response.ArchiveId.ToString()));
            }
        });
    }
}
