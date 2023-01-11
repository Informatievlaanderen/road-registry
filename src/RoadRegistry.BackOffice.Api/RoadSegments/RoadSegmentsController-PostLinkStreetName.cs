namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using Handlers.Sqs.RoadSegments;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Koppel een straatnaam aan een wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="ifMatchHeaderValidator"></param>
    /// <param name="parameters"></param>
    /// <param name="id">Identificator van het wegsegment.</param>
    /// <param name="ifMatchHeaderValue"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("{id}/acties/straatnaamkoppelen")]
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
    [SwaggerRequestExample(typeof(PostLinkStreetNameParameters), typeof(PostLinkStreetNameParametersExamples))]
    [SwaggerOperation(Description = "Koppel een linker- en/of rechterstraatnaam met status `voorgesteld` of `inGebruik` aan een wegsegment waaraan momenteel geen linker- en/of rechterstraatnaam gekoppeld werd.")]
    public async Task<IActionResult> PostLinkStreetName(
        [FromServices] UseRoadSegmentLinkStreetNameFeatureToggle featureToggle,
        [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
        [FromBody] PostLinkStreetNameParameters parameters,
        [FromRoute] int id,
        [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
        CancellationToken cancellationToken = default)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new RoadSegmentId(id), cancellationToken))
            {
                return new PreconditionFailedResult();
            }

            var result = await _mediator.Send(
                new LinkStreetNameSqsRequest
                {
                    Request = new LinkStreetNameRequest(id, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId),
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                }, cancellationToken);

            return Accepted(result);
        }
        catch (AggregateIdIsNotFoundException)
        {
            throw new ApiException(ValidationErrors.RoadSegment.NotFound.Message, StatusCodes.Status404NotFound);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

[DataContract(Name = "StraatnaamKoppelen", Namespace = "")]
public class PostLinkStreetNameParameters
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

public class PostLinkStreetNameParametersExamples : IExamplesProvider<PostUnlinkStreetNameParameters>
{
    public PostUnlinkStreetNameParameters GetExamples()
    {
        return new PostUnlinkStreetNameParameters
        {
            LinkerstraatnaamId = "https://data.vlaanderen.be/id/straatnaam/23489"
        };
    }
}
