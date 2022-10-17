namespace RoadRegistry.BackOffice.Api.Uploads;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using Framework;
using Infrastructure.FeatureToggles;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using Version = Infrastructure.Version;

[ApiVersion(Infrastructure.Version.Current)]
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
        }, cancellationToken);
    }

    [HttpPost("fc")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostUploadBeforeFeatureCompare([FromServices] UseFeatureCompareToggle useFeatureCompareToggle, IFormFile archive, CancellationToken cancellationToken)
    {
        if (!useFeatureCompareToggle.FeatureEnabled)
        {
            return NotFound();
        }

        return await Post(archive, async () =>
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractFeatureCompareRequest(archive.FileName, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }, cancellationToken);
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

    private static async Task<IActionResult> Post(IFormFile archive, Func<Task<IActionResult>> callback, CancellationToken cancellationToken)
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
        catch (ValidationException exception)
        {
            throw new ApiProblemDetailsException(
                "Could not upload roadnetwork extract because of validation errors",
                400,
                new ExceptionProblemDetails(exception), exception);
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
}
