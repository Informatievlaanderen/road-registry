namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadNetworks;
using FeatureToggles;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Snapshot.Handlers.Sqs.RoadNetworks;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string RebuildSnapshotRoute = "snapshots/refresh";

    /// <summary>
    ///     Rebuilds the snapshot.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="snapshotFeatureToggle">The snapshot feature toggle.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(RebuildSnapshotRoute, Name = nameof(RebuildSnapshot))]
    [SwaggerOperation(OperationId = nameof(RebuildSnapshot), Description = "")]
    public async Task<IActionResult> RebuildSnapshot([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] RebuildSnapshotParametersValidator validator,
        [FromServices] UseSnapshotSqsRequestFeatureToggle snapshotFeatureToggle,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        if (snapshotFeatureToggle.FeatureEnabled)
        {
            await Mediator.Send(new RebuildRoadNetworkSnapshotSqsRequest
            {
                Request = new RebuildRoadNetworkSnapshotRequest
                {
                    MaxStreamVersion = parameters.MaxStreamVersion
                }
            }, cancellationToken);
            return Accepted();
        }
        //TODO-rik test
        var command = new RebuildRoadNetworkSnapshot();
        await RoadNetworkCommandQueue
            .WriteAsync(new Command(command), cancellationToken);
        return Accepted();
    }
}

public class RebuildSnapshotParameters
{
    public int MaxStreamVersion { get; set; }
}

public class RebuildSnapshotParametersValidator : AbstractValidator<RebuildSnapshotParameters>
{
}
