namespace RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;

using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class GradeSeparatedJunctionsController
{
    private const string ChangeAttributesRoute = "{id}/acties/wijzigen/attributen";

    /// <summary>
    ///     Wijzig attribuutwaarde(n) voor een ongelijkgrondse kruising.
    /// </summary>
    /// <param name="id">De identificator van de ongelijkgrondse kruising.</param>
    /// <param name="parameters"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als de ongelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als de ongelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeAttributesRoute, Name = nameof(ChangeGradeSeparatedJunctionAttributesV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeSeparatedJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeSeparatedJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeGradeSeparatedJunctionAttributesV2Parameters), typeof(ChangeGradeSeparatedJunctionAttributesV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeGradeSeparatedJunctionAttributesV2), Description = "Wijzig attribuutwaarde(n) voor een ongelijkgrondse kruising: het onder- en bovenliggende wegsegment omwisselen, of het type aanpassen.")]
    public async Task<IActionResult> ChangeGradeSeparatedJunctionAttributesV2(
        [FromRoute] int id,
        [FromBody] ChangeGradeSeparatedJunctionAttributesV2Parameters parameters,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            if (!GradeSeparatedJunctionId.Accepts(id))
            {
                throw new ValidationException([new ValidationFailure("id", $"De waarde {id} is ongeldig.")]);
            }

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. Whether the named road segments are this crossing's, and whether they differ, is left to
            // the domain: it is the only place that knows what the junction is about.
            var gradeSeparatedJunction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(id, cancellationToken);
            if (gradeSeparatedJunction is null)
            {
                return NotFound();
            }

            if (gradeSeparatedJunction.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var (lowerRoadSegmentId, upperRoadSegmentId, type) = OngelijkgrondseKruisingAttribuutParameters.TranslateAndValidate(
                parameters?.OnderliggendWegsegment,
                parameters?.BovenliggendWegsegment,
                parameters?.OngelijkgrondseKruisingType);

            var sqsRequest = new ChangeGradeSeparatedJunctionAttributesV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(id),
                LowerRoadSegmentId = lowerRoadSegmentId,
                UpperRoadSegmentId = upperRoadSegmentId,
                Type = type
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

/// <summary>
///     De attribuutwaarden van de ongelijkgrondse kruising.
/// </summary>
[DataContract(Name = "OngelijkgrondseKruisingV2AttribuutwaardenWijzigen", Namespace = "")]
public record ChangeGradeSeparatedJunctionAttributesV2Parameters
{
    /// <summary>
    ///     Objectidentificator van het onderliggende wegsegment van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "OnderliggendWegsegment", Order = 0)]
    [JsonProperty("onderliggendWegsegment", Required = Required.Always)]
    public string? OnderliggendWegsegment { get; set; }

    /// <summary>
    ///     Objectidentificator van het bovenliggende wegsegment van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "BovenliggendWegsegment", Order = 1)]
    [JsonProperty("bovenliggendWegsegment", Required = Required.Always)]
    public string? BovenliggendWegsegment { get; set; }

    /// <summary>
    ///     Type van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "OngelijkgrondseKruisingType", Order = 2)]
    [JsonProperty("ongelijkgrondseKruisingType", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(GradeSeparatedJunctionTypeV2.Edit))]
    public string? OngelijkgrondseKruisingType { get; set; }
}

public class ChangeGradeSeparatedJunctionAttributesV2ParametersExamples : IExamplesProvider<ChangeGradeSeparatedJunctionAttributesV2Parameters>
{
    public ChangeGradeSeparatedJunctionAttributesV2Parameters GetExamples()
    {
        return new ChangeGradeSeparatedJunctionAttributesV2Parameters
        {
            OnderliggendWegsegment = "432756",
            BovenliggendWegsegment = "987567",
            OngelijkgrondseKruisingType = GradeSeparatedJunctionTypeV2.Tunnel.ToDutchString()
        };
    }
}
