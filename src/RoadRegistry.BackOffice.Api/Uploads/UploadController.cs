namespace RoadRegistry.BackOffice.Api.Uploads;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Contracts;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using RoadRegistry.BackOffice.Contracts.Uploads;

[ApiVersion("1.0")]
[AdvertiseApiVersions("1.0")]
[ApiRoute("upload")]
[ApiExplorerSettings(GroupName = "Uploads")]
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadController(IMediator mediator) => _mediator = mediator;

    [HttpPost("")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<ActionResult> PostUpload([FromBody] IFormFile archive, [FromBody] bool featureCompare, CancellationToken cancellationToken)
    {
        UploadExtractRequest request = new(archive.FileName, new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType)));

        try
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for superseded download",
                409, new ExceptionProblemDetails(exception), exception);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for same download more than once",
                409, new ExceptionProblemDetails(exception), exception);
        }
    }

    [HttpGet("{identifier}")]
    public async Task<ActionResult> Get(string identifier, CancellationToken cancellationToken)
    {
        UploadStatusRequest request = new(identifier);
        var response = await _mediator.Send(request, cancellationToken);
        return new FileCallbackResult(response);
    }
}
