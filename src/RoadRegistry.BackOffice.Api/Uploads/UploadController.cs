namespace RoadRegistry.BackOffice.Api.Uploads;

using System;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Core.ProblemCodes;
using FluentValidation;
using FluentValidation.Results;
using Hosts.Infrastructure.Options;
using Infrastructure.Controllers;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("upload")]
[ApiExplorerSettings(GroupName = "Upload")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class UploadController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public UploadController(TicketingOptions ticketingOptions, IMediator mediator) : base(ticketingOptions)
    {
        _mediator = mediator;
    }

    private static async Task<IActionResult> PostUpload(IFormFile archive, Func<Task<IActionResult>> callback)
    {
        if (archive == null)
        {
            throw new ValidationException("Archive is required", new[]
            {
                new ValidationFailure
                {
                    PropertyName = nameof(archive),
                    ErrorCode = ProblemCode.Common.IsRequired
                }
            });
        }

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
}