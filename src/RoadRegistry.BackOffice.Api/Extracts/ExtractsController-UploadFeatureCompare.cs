namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
            if (useZipArchiveFeatureCompareTranslatorFeatureToggle.FeatureEnabled)
            {
                UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
                var request = new UploadExtractRequest(archive.FileName, requestArchive)
                {
                    UseZipArchiveFeatureCompareTranslator = useZipArchiveFeatureCompareTranslatorFeatureToggle.FeatureEnabled
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

public record UploadExtractFeatureCompareResponseBody(string ArchiveId);