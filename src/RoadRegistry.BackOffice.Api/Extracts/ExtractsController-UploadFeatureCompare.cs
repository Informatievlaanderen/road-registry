namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UploadExtractFeatureCompareRequest = Abstractions.Extracts.UploadExtractFeatureCompareRequest;
using UploadExtractRequest = Abstractions.Extracts.UploadExtractRequest;

public partial class ExtractsController
{
    private const string PostFeatureCompareUploadRoute = "download/{downloadId}/uploads/fc";

    /// <summary>
    ///     Upload before feature compare archive.
    /// </summary>
    /// <param name="useZipArchiveFeatureCompareTranslatorFeatureToggle"></param>
    /// <param name="downloadId"></param>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>Task&lt;IActionResult&gt;.</returns>
    [HttpPost(PostFeatureCompareUploadRoute, Name = nameof(UploadFeatureCompare))]
    [SwaggerOperation(OperationId = nameof(UploadFeatureCompare), Description = "")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public Task<IActionResult> UploadFeatureCompare(
        [FromServices] UseZipArchiveFeatureCompareTranslatorFeatureToggle useZipArchiveFeatureCompareTranslatorFeatureToggle,
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        return PostUpload(archive, async () =>
        {
            if (GetFeatureToggleValue(useZipArchiveFeatureCompareTranslatorFeatureToggle))
            {
                var response = await _mediator.Send(
                    new UploadExtractRequest(
                        downloadId,
                        new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType))
                    ) { UseZipArchiveFeatureCompareTranslator = useZipArchiveFeatureCompareTranslatorFeatureToggle.FeatureEnabled }, cancellationToken);
                return Accepted(new UploadExtractFeatureCompareResponseBody(response.UploadId.ToString()));
            }
            else
            {
                var response = await _mediator.Send(
                    new UploadExtractFeatureCompareRequest(
                        downloadId,
                        new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType))
                    ), cancellationToken);
                return Accepted(new UploadExtractFeatureCompareResponseBody(response.UploadId.ToString()));
            }
        });
    }
}

public record UploadExtractFeatureCompareResponseBody(string UploadId);
