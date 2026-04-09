namespace RoadRegistry.BackOffice.Api.Files;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using RoadRegistry.BackOffice.Abstractions.Uploads;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Jobs;

public partial class FilesController
{
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("upload/{jobId:guid}")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Upload(
        [FromRoute] Guid jobId,
        [FromForm(Name = "file")] IFormFile archive,
        CancellationToken cancellationToken,
        [FromServices] JobsContext jobsContext,
        [FromServices] RoadNetworkJobsBlobClient jobsBlobClient,
        [FromServices] IHostEnvironment hostEnvironment)
    {
        // Only exists for local dev testing the entire flow (in UI)
        if (!hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }

        var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
        await Handle(jobId, requestArchive, jobsContext, jobsBlobClient, cancellationToken);

        return NoContent();
    }

    public async Task Handle(
        Guid jobId,
        UploadExtractArchiveRequest archiveRequest,
        JobsContext jobsContext,
        RoadNetworkJobsBlobClient jobsBlobClient,
        CancellationToken cancellationToken)
    {
        var job = await jobsContext.FindJob(jobId, cancellationToken);
        if (job is null)
        {
            throw new ApiException("Onbestaande upload job.", StatusCodes.Status404NotFound);
        }

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(archiveRequest.FileName)
                    ? job.Id + ".zip"
                    : archiveRequest.FileName)
        );

        await jobsBlobClient.CreateBlobAsync(
            new BlobName(job.ReceivedBlobName),
            metadata,
            ContentType.Parse("application/zip"),
            archiveRequest.ReadStream,
            cancellationToken
        );
    }
}
