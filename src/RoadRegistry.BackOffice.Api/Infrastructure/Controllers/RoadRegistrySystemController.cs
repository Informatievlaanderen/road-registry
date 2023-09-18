namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Attributes;
using Authentication;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiExplorerSettings(GroupName = "Infrastructuur")]
[ApiKey(Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
public partial class RoadRegistrySystemController : BackofficeApiController
{
    public RoadRegistrySystemController(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkCommandQueue roadNetworkCommandQueue)
    {
        Store = store;
        Mediator = mediator;
        RoadNetworkCommandQueue = roadNetworkCommandQueue;
    }

    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IRoadNetworkCommandQueue RoadNetworkCommandQueue { get; }
}
