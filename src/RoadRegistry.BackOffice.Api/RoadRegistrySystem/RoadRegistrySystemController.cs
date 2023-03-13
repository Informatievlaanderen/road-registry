namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class RoadRegistrySystemController : ControllerBase
{
    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IRoadNetworkCommandQueue CommandQueue { get; }

    public RoadRegistrySystemController(
        IStreamStore store,
        IMediator mediator)
    {
        Store = store;
        Mediator = mediator;
        CommandQueue = new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.BackOffice));
    }
}
