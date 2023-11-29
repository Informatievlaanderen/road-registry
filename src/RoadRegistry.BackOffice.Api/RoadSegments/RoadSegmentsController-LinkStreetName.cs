namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.Extensions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Infrastructure;
using Infrastructure.Authentication;
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
    private const string LinkStreetNameRoute = "{id}/acties/straatnaamkoppelen";

    /// <summary>
    ///     Koppel een straatnaam aan een wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="ifMatchHeaderValidator"></param>
    /// <param name="idValidator"></param>
    /// <param name="validator"></param>
    /// <param name="roadSegmentRepository"></param>
    /// <param name="parameters"></param>
    /// <param name="id">Identificator van het wegsegment.</param>
    /// <param name="ifMatchHeaderValue"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(LinkStreetNameRoute, Name = nameof(LinkStreetName))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
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
    [SwaggerOperation(OperationId = nameof(LinkStreetName), Description = "Koppel een linker- en/of rechterstraatnaam met status `voorgesteld` of `inGebruik` aan een wegsegment waaraan momenteel geen linker- en/of rechterstraatnaam gekoppeld werd.")]
    public async Task<IActionResult> LinkStreetName(
        [FromServices] UseRoadSegmentLinkStreetNameFeatureToggle featureToggle,
        [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromServices] IValidator<LinkStreetNameRequest> validator,
        [FromServices] IRoadSegmentRepository roadSegmentRepository,
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
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            var roadSegment = await roadSegmentRepository.Find(new RoadSegmentId(id), cancellationToken);
            if (roadSegment is null)
            {
                return NotFound();
            }

            if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, roadSegment, cancellationToken))
            {
                return new PreconditionFailedResult();
            }

            var request = new LinkStreetNameRequest(id, roadSegment.GeometryDrawMethod, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await _mediator.Send(new LinkStreetNameSqsRequest { Request = request }, cancellationToken);

            return Accepted(result);
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

public class PostLinkStreetNameParametersExamples : IExamplesProvider<PostLinkStreetNameParameters>
{
    public PostLinkStreetNameParameters GetExamples()
    {
        return new PostLinkStreetNameParameters
        {
            LinkerstraatnaamId = "https://data.vlaanderen.be/id/straatnaam/23489"
        };
    }
}
