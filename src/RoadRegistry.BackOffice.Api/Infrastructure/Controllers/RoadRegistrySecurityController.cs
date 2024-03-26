namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("security")]
[ApiExplorerSettings(GroupName = "Infrastructuur")]
public partial class RoadRegistrySecurityController : BackofficeApiController
{
    private readonly ILogger _logger;
    private readonly OpenIdConnectOptions _openIdConnectOptions;

    public RoadRegistrySecurityController(
        OpenIdConnectOptions openIdConnectOptions,
        ILogger<RoadRegistrySecurityController> logger)
    {
        _openIdConnectOptions = openIdConnectOptions;
        _logger = logger;
    }
}
