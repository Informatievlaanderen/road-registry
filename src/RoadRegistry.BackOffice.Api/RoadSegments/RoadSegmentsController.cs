namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Be.Vlaanderen.Basisregisters.Api;
using Hosts.Infrastructure.Options;
using Infrastructure;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "Wegsegmenten")]
public partial class RoadSegmentsController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(TicketingOptions ticketingOptions, IMediator mediator)
        : base(ticketingOptions)
    {
        _mediator = mediator;
    }
}
