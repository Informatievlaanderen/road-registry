namespace RoadRegistry.BackOffice.Api.Uploads;

using Abstractions.Exceptions;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api;
using Core.ProblemCodes;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Hosts.Infrastructure.Options;
using Infrastructure.Authentication;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("upload")]
[ApiExplorerSettings(GroupName = "Upload")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
public partial class UploadController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public UploadController(TicketingOptions ticketingOptions, IMediator mediator) : base(ticketingOptions)
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
        catch (ExtractRequestMarkedInformativeException)
        {
            throw new ValidationException("The roadnetwork extract is marked informative. Upload not allowed.", new[]
            {
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = ProblemCode.Upload.UploadNotAllowedForInformativeExtract
                }
            });
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException)
        {
            throw new ValidationException("Can not upload roadnetwork extract changes archive for superseded download.", new[]
            {
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload
                }
            });
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException)
        {
            throw new ValidationException("Can not upload roadnetwork extract changes archive for same download more than once.", new[]
            {
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce
                }
            });
        }
    }
}
