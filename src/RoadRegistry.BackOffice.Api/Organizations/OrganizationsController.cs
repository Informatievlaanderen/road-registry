namespace RoadRegistry.BackOffice.Api.Organizations;

using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Controllers;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("organizations")]
[ApiExplorerSettings(GroupName = "Organisaties")]
[ApiKey]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
public partial class OrganizationsController : BackofficeApiController
{
    public OrganizationsController(
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
