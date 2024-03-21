namespace RoadRegistry.BackOffice.Api.Information;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Info")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = AcmIdmConstants.PolicyNames.VoInfo)]
public partial class InformationController : ApiController
{
    private readonly IMediator _mediator;

    public InformationController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
