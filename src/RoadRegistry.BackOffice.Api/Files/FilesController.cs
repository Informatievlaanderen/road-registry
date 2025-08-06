namespace RoadRegistry.BackOffice.Api.Files;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("files")]
[ApiExplorerSettings(IgnoreApi = true)]
public partial class FilesController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public FilesController(BackofficeApiControllerContext apiContext, IMediator mediator) : base(apiContext)
    {
        _mediator = mediator;
    }
}
