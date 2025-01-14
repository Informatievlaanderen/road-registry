namespace RoadRegistry.BackOffice.Api.Grb;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Infrastructure.Authentication;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("grb")]
[ApiExplorerSettings(GroupName = "Grb")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
public partial class GrbController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public GrbController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
