namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Api;
using Core;
using FluentValidation;
using Infrastructure.Controllers.Attributes;
using Messages;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public class RoadRegistrySystemController : ControllerBase
{
    private readonly IValidator<RebuildSnapshotParameters> _rebuildSnapshotParametersValidator;
    private readonly UseSnapshotRebuildFeatureToggle _snapshotRebuildFeatureToggle;
    private readonly IStreamStore _store;

    public RoadRegistrySystemController(
        IStreamStore store,
        IValidator<RebuildSnapshotParameters> rebuildSnapshotParametersValidator,
        UseSnapshotRebuildFeatureToggle snapshotRebuildFeatureToggle)
    {
        _store = store;
        _rebuildSnapshotParametersValidator = rebuildSnapshotParametersValidator;
        _snapshotRebuildFeatureToggle = snapshotRebuildFeatureToggle;
    }

    [HttpPatch("organization/rename")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> RequestOrganizationRename([FromBody] RenameOrganizationParameters parameters,
        [FromServices] UseOrganizationRenameFeatureToggle featureToggle,
        [FromServices] IValidator<RenameOrganization> validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
        
        var command = new RenameOrganization
        {
            Code = parameters?.Code,
            Name = parameters?.Name
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await new RoadNetworkCommandQueue(_store)
            .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }

    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> RequestSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters)
    {
        if (!_snapshotRebuildFeatureToggle.FeatureEnabled)
        {
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        var validationResult = await _rebuildSnapshotParametersValidator.ValidateAsync(parameters, HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            return BadRequest();
        }

        var command = new RebuildRoadNetworkSnapshot
        {
            StartFromVersion = parameters.StartFromVersion
        };
        await new RoadNetworkCommandQueue(_store)
            .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }
}
