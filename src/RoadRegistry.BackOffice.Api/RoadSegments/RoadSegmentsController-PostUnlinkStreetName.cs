namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Ontkoppel een straatnaam van een wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="ifMatchHeaderValidator"></param>
    /// <param name="parameters"></param>
    /// <param name="id">Identificator van het wegsegment.</param>
    /// <param name="ifMatchHeaderValue"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("{id}/acties/straatnaamontkoppelen")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostUnlinkStreetNameParameters), typeof(PostUnlinkStreetNameParametersExamples))]
    [SwaggerOperation(Description = "Ontkoppel een linker- en/of rechterstraatnaam van een wegsegment waaraan momenteel een linker- en/of rechterstraatnaam gekoppeld is.")]
    public async Task<IActionResult> PostUnlinkStreetName(
        [FromServices] UseRoadSegmentUnlinkStreetNameFeatureToggle featureToggle,
        [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
        [FromServices] IValidator<UnlinkStreetNameRequest> validator,
        [FromBody] PostUnlinkStreetNameParameters parameters,
        [FromRoute] int id,
        [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            var request = new UnlinkStreetNameRequest(id, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new RoadSegmentId(id), cancellationToken))
            {
                return new PreconditionFailedResult();
            }

            var result = await _mediator.Send(Enrich(
                new UnlinkStreetNameSqsRequest
                {
                    Request = request
                }), cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

[DataContract(Name = "StraatnaamOntkoppelen", Namespace = "")]
public class PostUnlinkStreetNameParameters
{
    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de linkerzijde van het wegsegment.
    /// </summary>
    [DataMember(Name = "LinkerstraatnaamId", Order = 1)]
    [JsonProperty]
    public string LinkerstraatnaamId { get; set; }

    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de rechterzijde van het wegsegment.
    /// </summary>
    [DataMember(Name = "RechterstraatnaamId", Order = 2)]
    [JsonProperty]
    public string RechterstraatnaamId { get; set; }
}

public class PostUnlinkStreetNameParametersExamples : IExamplesProvider<PostUnlinkStreetNameParameters>
{
    public PostUnlinkStreetNameParameters GetExamples()
    {
        return new PostUnlinkStreetNameParameters
        {
            LinkerstraatnaamId = "https://data.vlaanderen.be/id/straatnaam/23489"
        };
    }
}
