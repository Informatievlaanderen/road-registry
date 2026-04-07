namespace RoadRegistry.BackOffice.Api.Inwinning;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using Version = RoadRegistry.BackOffice.Api.Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.Current)]
[ApiRoute("inwinningsstatus")]
[ApiExplorerSettings(GroupName = "Inwinningsstatus")]
[AllowAnonymous]
public partial class InwinningsstatusController : BackofficeApiController
{
    public InwinningsstatusController(BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
    }
}
