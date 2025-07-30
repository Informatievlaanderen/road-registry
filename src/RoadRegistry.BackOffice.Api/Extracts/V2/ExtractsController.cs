namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Asp.Versioning;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Core.ProblemCodes;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Authentication;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.V2)]
[AdvertiseApiVersions(Version.V2)]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "Extract")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
public partial class ExtractsController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public ExtractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private async Task<IActionResult> PostUpload(IFormFile archive, Func<Task<IActionResult>> callback)
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
        catch (DownloadExtractNotFoundException)
        {
            return NotFound();
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
        catch (ExtractRequestMarkedInformativeException ex)
        {
            throw new ApiProblemDetailsException(
                "The roadnetwork extract is marked informative. Upload not allowed.",
                409,
                new ExceptionProblemDetails(ex), ex);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException ex)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for superseded download",
                409,
                new ExceptionProblemDetails(ex), ex);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException ex)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for same download more than once",
                409,
                new ExceptionProblemDetails(ex), ex);
        }
    }
}
