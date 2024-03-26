namespace RoadRegistry.BackOffice.Api.Downloads;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("download")]
[ApiExplorerSettings(GroupName = "Download")]
public partial class DownloadController : ApiController
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
