namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class RoadRegistrySystemController : BackofficeApiController
{
    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IRoadNetworkCommandQueue RoadNetworkCommandQueue { get; }

    public RoadRegistrySystemController(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkCommandQueue roadNetworkCommandQueue)
    {
        Store = store;
        Mediator = mediator;
        RoadNetworkCommandQueue = roadNetworkCommandQueue;
    }
}
