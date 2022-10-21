namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("upload")]
[ApiExplorerSettings(GroupName = "Uploads")]
[ApiKeyAuth("Road")]
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{identifier}")]
    public async Task<IActionResult> Get(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            DownloadExtractRequest request = new(identifier);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }

    private static async Task<IActionResult> Post(IFormFile archive, Func<Task<IActionResult>> callback)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));

        try
        {
            return await callback.Invoke();
        }
        catch (UnsupportedMediaTypeException)
        {
            return new UnsupportedMediaTypeResult();
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for superseded download",
                409,
                new ExceptionProblemDetails(exception), exception);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for same download more than once",
                409,
                new ExceptionProblemDetails(exception), exception);
        }
    }

    [HttpPost("")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostUploadAfterFeatureCompare(IFormFile archive, CancellationToken cancellationToken)
    {
        return await Post(archive, async () =>
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractRequest(archive.FileName, requestArchive);
            await _mediator.Send(request, cancellationToken);
            return Ok();
        });
    }

    [HttpPost("fc")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostUploadBeforeFeatureCompare([FromServices] UseFeatureCompareFeatureToggle useFeatureCompareToggle, IFormFile archive, CancellationToken cancellationToken)
    {
        if (!useFeatureCompareToggle.FeatureEnabled) return NotFound();

        return await Post(archive, async () =>
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractFeatureCompareRequest(archive.FileName, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        });
    }
}

internal class ApiValidationProblemDetailsException : ApiProblemDetailsException
{
    public ApiValidationProblemDetailsException(string message, int statusCode, ProblemDetails problemDetails, ValidationException innerException)
        : base(message, statusCode, problemDetails, innerException)
    {
        ValidationErrors = innerException.Errors;
    }

    public IEnumerable<ValidationFailure> ValidationErrors { get; init; }
}
