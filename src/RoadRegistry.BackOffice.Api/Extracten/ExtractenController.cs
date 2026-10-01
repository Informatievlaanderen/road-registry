namespace RoadRegistry.BackOffice.Api.Extracten;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using Version = Infrastructure.Version;

// V1 serves extracten in the old datamodel, V2 the hernieuwde one. Most of what the controller does is the same either
// way and is mapped to both; the four endpoints that differ - the three downloadaanvragen and the upload - carry a
// MapToApiVersion and have a V2 twin next to them.
[ApiVersion(Version.V1)]
[ApiVersion(Version.V2)]
[ApiRoute("extracten")]
[ApiExplorerSettings(GroupName = "Extracten")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
public partial class ExtractenController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public ExtractenController(BackofficeApiControllerContext apiContext, IMediator mediator)
        : base(apiContext)
    {
        _mediator = mediator;
    }
}
