namespace RoadRegistry.BackOffice.Api.Inwinning;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Sync.MunicipalityRegistry;
using RoadRegistry.Sync.MunicipalityRegistry.Models;
using Swashbuckle.AspNetCore.Annotations;

public partial class InwinningsstatusController
{
    /// <summary>
    ///     Get gemeente inwinningsstatus
    /// </summary>
    /// <param name="nisCode"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="municipalityContext"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(GemeenteInwinningsstatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetGemeenteInwinningsstatus))]
    [HttpGet("gemeente/{nisCode}", Name = nameof(GetGemeenteInwinningsstatus))]
    [AllowAnonymous]
    public async Task<IActionResult> GetGemeenteInwinningsstatus(
        [FromRoute] string nisCode,
        [FromServices] ExtractsDbContext extractsDbContext,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        CancellationToken cancellationToken = default)
    {
        var inwinningszone = await extractsDbContext.Inwinningszones
            .Where(x => x.NisCode == nisCode)
            .SingleOrDefaultAsync(cancellationToken);

        if (inwinningszone is null)
        {
            var municipality = await municipalityContext.Municipalities.SingleOrDefaultAsync(x => x.NisCode == nisCode && x.Status == MunicipalityStatus.Current, cancellationToken);
            if (municipality is null)
            {
                return NotFound();
            }

            return Ok(new GemeenteInwinningsstatus
            {
                Inwinningsstatus = Inwinningsstatus.NietGestart
            });
        }

        //TODO-pr add historiek niet-informatieve extracten
        /*
         "historiekExtracten":
         [
             {
                "downloadId": "4886456fb7604ab5b065cee5419907c3",
                "historiek": [
                   {
                       "status": "beschikbaar",
                       "datum": "03-12-2026"
                   },
                   {
                       "status": "verworpen",
                       "datum": "07-02-2027"
                   },
                   {
                       "status": "automatische controles geslaagd",
                       "datum": "08-02-2027"
                   },
                   {
                       "status": "goedgekeurd",
                       "datum": "22-02-2027"
                   }
                ]
            }
        ]
        */

        return Ok(new GemeenteInwinningsstatus
        {
            Inwinningsstatus = inwinningszone.Completed
                ? Inwinningsstatus.Compleet
                : Inwinningsstatus.Locked
        });
    }
}

public class GemeenteInwinningsstatus
{
    [RoadRegistryEnumDataType(typeof(Inwinningsstatus))]
    public string Inwinningsstatus { get; set; }
}
