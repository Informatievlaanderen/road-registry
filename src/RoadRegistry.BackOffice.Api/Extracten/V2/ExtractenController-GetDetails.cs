namespace RoadRegistry.BackOffice.Api.Extracten;

using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Extensions;
using Swashbuckle.AspNetCore.Annotations;
using Version = Infrastructure.Version;

public partial class ExtractenController
{
    /// <summary>
    ///     Vraagt de details van een extract op, met de contour in Lambert 2008.
    /// </summary>
    [ProducesResponseType(typeof(ExtractDetailsResponseBody), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetExtractDetailsV2))]
    [MapToApiVersion(Version.V2)]
    [HttpGet("{downloadId}", Name = nameof(GetExtractDetailsV2))]
    public Task<ActionResult> GetExtractDetailsV2(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken = default)
    {
        // The v2 endpoints work in Lambert 2008 only, whichever datamodel the extract itself is in.
        return GetExtractDetails(downloadId, contour => contour.EnsureLambert08(), cancellationToken);
    }
}
