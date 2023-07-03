namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Be.Vlaanderen.Basisregisters.Api;
using Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("security")]
[ApiExplorerSettings(GroupName = "Infrastructuur")]
public partial class RoadRegistrySecurityController : BackofficeApiController
{
    private readonly ILogger _logger;
    private readonly OpenIdConnectOptions _openIdConnectOptions;

    public RoadRegistrySecurityController(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        OpenIdConnectOptions openIdConnectOptions,
        ILogger<RoadRegistrySecurityController> logger)
    {
        Store = store;
        Mediator = mediator;
        RoadNetworkCommandQueue = roadNetworkCommandQueue;
        _openIdConnectOptions = openIdConnectOptions;
        _logger = logger;
    }

    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IRoadNetworkCommandQueue RoadNetworkCommandQueue { get; }
}
