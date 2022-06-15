namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem
{
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.Api;
    using FluentValidation;
    using System.Threading.Tasks;
    using Hosts;
    using Messages;
    using Microsoft.AspNetCore.Mvc;
    using SqlStreamStore;

    [ApiVersion("1.0")]
    [ApiRoute("system")]
    public class RoadRegistrySystemController : ControllerBase
    {
        private readonly IStreamStore _store;
        private readonly IValidator<RebuildSnapshotParameters> _rebuildSnapshotParametersValidator;
        private readonly UseSnapshotRebuildFeatureToggle _snapshotRebuildFeatureToggle;

        public RoadRegistrySystemController(
            IStreamStore store,
            IValidator<RebuildSnapshotParameters> rebuildSnapshotParametersValidator,
            UseSnapshotRebuildFeatureToggle snapshotRebuildFeatureToggle)
        {
            _store = store;
            _rebuildSnapshotParametersValidator = rebuildSnapshotParametersValidator;
            _snapshotRebuildFeatureToggle = snapshotRebuildFeatureToggle;
        }

        [HttpPost("snapshots/refresh")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> RequestSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters)
        {
            if (!_snapshotRebuildFeatureToggle.FeatureEnabled)
            {
                return StatusCode(501); // Not Implemented
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
}
