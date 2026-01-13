namespace RoadRegistry.BackOffice.Api.Inwinning;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;

public partial class InwinningController
{
    /// <summary>
    ///     Get available nis-codes
    /// </summary>
    /// <param name="options"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetNisCodes))]
    [HttpGet("niscodes", Name = nameof(GetNisCodes))]
    public async Task<IActionResult> GetNisCodes(
        [FromServices] IOptions<InwinningOrganizationNisCodesOptions> options,
        [FromServices] ExtractsDbContext extractsDbContext,
        CancellationToken cancellationToken = default)
    {
        var @operator = ApiContext.HttpContextAccessor.HttpContext.GetOperator();
        if (@operator is null || !options.Value.TryGetValue(@operator, out var niscodes) )
        {
            return Ok(Array.Empty<string>());
        }

        var excludeNisCodes = await extractsDbContext.Inwinningszones
            .Where(x => x.Completed || x.Operator != @operator)
            .Select(x => x.NisCode)
            .ToListAsync(cancellationToken);

        return Ok(niscodes.Except(excludeNisCodes).ToList());
    }
}
