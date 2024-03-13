namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Infrastructure;
using Infrastructure.Query;
using Jobs.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

public partial class UploadController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip te uploaden.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(GetPresignedUploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FileCallbackResultExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetUploadPreSignedUrl), Description = "")]
    [HttpPost("jobs", Name = nameof(GetUploadPreSignedUrl))]
    public async Task<IActionResult> GetUploadPreSignedUrl(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(GetPresignedUploadUrlRequest.ForUploads(), cancellationToken));
    }

    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("jobs/{jobId:guid}/upload")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UploadJob(
        [FromRoute] Guid jobId,
        IFormFile archive,
        CancellationToken cancellationToken,
        [FromServices] IHostEnvironment hostEnvironment)
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return NotFound();
        }

        var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
        var request = new JobUploadArchiveRequest(jobId, requestArchive);

        await _mediator.Send(request, cancellationToken);

        return Accepted();
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs([FromQuery] string statuses, CancellationToken cancellationToken)
    {
        var pagination = new Pagination(HttpContext.Request.Query);
        var statusesFilter = new EnumFilter<Jobs.JobStatus>(HttpContext.Request.Query, nameof(statuses));

        return Ok(await _mediator.Send(new GetJobsRequest(pagination, statusesFilter), cancellationToken));
    }

    [HttpGet("jobs/active")]
    public async Task<IActionResult> GetActiveJobs(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetActiveJobsRequest(), cancellationToken));
    }

    [HttpGet("jobs/{jobId:guid}")]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetJobByIdRequest(jobId), cancellationToken));
    }

    [HttpDelete("jobs/{jobId:guid}")]
    public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelJobRequest(jobId), cancellationToken);
        return NoContent();
    }
}
