namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Asp.Versioning;
using Authentication;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiExplorerSettings(GroupName = "Infrastructuur")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
public partial class RoadRegistrySystemController : BackofficeApiController
{
    public RoadRegistrySystemController(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
        Store = store;
        Mediator = mediator;
        RoadNetworkCommandQueue = roadNetworkCommandQueue;
    }

    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IRoadNetworkCommandQueue RoadNetworkCommandQueue { get; }
}
