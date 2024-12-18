namespace RoadRegistry.BackOffice.Api.Organizations;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("organizations")]
[ApiExplorerSettings(GroupName = "Organisaties")]
public partial class OrganizationsController : BackofficeApiController
{
    public OrganizationsController(
        IStreamStore store,
        IMediator mediator,
        IOrganizationCommandQueue organizationCommandQueue)
    {
        Store = store;
        Mediator = mediator;
        OrganizationCommandQueue = organizationCommandQueue;
    }

    protected IStreamStore Store { get; }
    protected IMediator Mediator { get; }
    protected IOrganizationCommandQueue OrganizationCommandQueue { get; }
}
