namespace RoadRegistry.BackOffice.Api.Information;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Info")]
[ApiKey(Policy = AcmIdmConstants.PolicyNames.VoInfo)]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = AcmIdmConstants.PolicyNames.VoInfo)]
public partial class InformationController : ApiController
{
    private readonly IMediator _mediator;

    public InformationController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
