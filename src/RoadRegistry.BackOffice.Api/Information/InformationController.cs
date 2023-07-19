namespace RoadRegistry.BackOffice.Api.Information;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Authorization;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiKey]
[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Info")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = AcmIdmConstants.PolicyNames.Authenticated)]
public partial class InformationController : ApiController
{
    private readonly IMediator _mediator;

    public InformationController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
