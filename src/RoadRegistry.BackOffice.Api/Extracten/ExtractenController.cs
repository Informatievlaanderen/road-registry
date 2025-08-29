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

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.Current)]
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
