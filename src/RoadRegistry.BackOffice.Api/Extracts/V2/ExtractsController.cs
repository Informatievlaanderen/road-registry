namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Infrastructure.Authentication;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.V2)]
[AdvertiseApiVersions(Version.V2)]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "ExtractV2")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
public partial class ExtractsController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public ExtractsController(BackofficeApiControllerContext apiContext, IMediator mediator)
        : base(apiContext)
    {
        _mediator = mediator;
    }
}
