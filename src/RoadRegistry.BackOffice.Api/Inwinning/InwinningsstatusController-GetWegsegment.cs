namespace RoadRegistry.BackOffice.Api.Inwinning;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;

public partial class InwinningsstatusController
{
    /// <summary>
    ///     Get wegsegment inwinningsstatus
    /// </summary>
    /// <param name="id"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="documentStore"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(WegsegmentInwinningsstatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetWegsegmentInwinningsstatus))]
    [HttpGet("wegsegment/{id}", Name = nameof(GetWegsegmentInwinningsstatus))]
    [AllowAnonymous]
    public async Task<IActionResult> GetWegsegmentInwinningsstatus(
        [FromRoute] int id,
        [FromServices] ExtractsDbContext extractsDbContext,
        [FromServices] Marten.IDocumentStore documentStore,
        CancellationToken cancellationToken = default)
    {
        var inwinningRoadSegment = await extractsDbContext.InwinningRoadSegments
            .Where(x => x.RoadSegmentId == id)
            .SingleOrDefaultAsync(cancellationToken);

        if (inwinningRoadSegment is null)
        {
            await using var session = documentStore.LightweightSession();

            var roadSegment = await session.LoadAsync<RoadSegmentExtractItem>(id, cancellationToken);
            if (roadSegment is null)
            {
                return NotFound();
            }

            return Ok(new WegsegmentInwinningsstatus
            {
                Inwinningsstatus = Inwinningsstatus.NietGestart
            });
        }

        return Ok(new WegsegmentInwinningsstatus
        {
            Inwinningsstatus = inwinningRoadSegment.Completed
                ? Inwinningsstatus.Compleet
                : Inwinningsstatus.Locked
        });
    }
}

public class WegsegmentInwinningsstatus
{
    [RoadRegistryEnumDataType(typeof(Inwinningsstatus))]
    public string Inwinningsstatus { get; init; }
}
