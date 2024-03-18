namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Abstractions.RoadNetworks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Snapshot.Handlers.Sqs.RoadNetworks;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string RebuildSnapshotRoute = "snapshots/refresh";

    /// <summary>
    ///     Rebuilds the snapshot.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(RebuildSnapshotRoute, Name = nameof(RebuildSnapshot))]
    [SwaggerOperation(OperationId = nameof(RebuildSnapshot), Description = "")]
    public async Task<IActionResult> RebuildSnapshot([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] RebuildSnapshotParametersValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        var result = await Mediator.Send(new RebuildRoadNetworkSnapshotSqsRequest
        {
            Request = new RebuildRoadNetworkSnapshotRequest
            {
                MaxStreamVersion = parameters.MaxStreamVersion
            }
        }, cancellationToken);
        return Accepted(result);
    }
}

public class RebuildSnapshotParameters
{
    public int MaxStreamVersion { get; set; }
}

public class RebuildSnapshotParametersValidator : AbstractValidator<RebuildSnapshotParameters>
{
}
