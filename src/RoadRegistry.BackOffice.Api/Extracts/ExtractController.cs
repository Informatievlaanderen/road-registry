namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Api;
using Core.ProblemCodes;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "Extract")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class ExtractController : ApiController
{
    private readonly IMediator _mediator;

    public ExtractController(IMediator mediator)
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
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }
}