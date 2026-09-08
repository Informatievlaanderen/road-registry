namespace RoadRegistry.BackOffice.Api.V2.GradeJunctions;

using System;
using System.Collections.Generic;
using System.Linq;
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
using RoadRegistry.BackOffice.Handlers.Sqs.GradeJunctions.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class GradeJunctionsController
{
    private const string ChangeToGradeSeparatedJunctionRoute = "{id}/acties/wijzigen/naarongelijkgrondsekruising";

    /// <summary>
    ///     Wijzig een gelijkgrondse kruising naar een ongelijkgrondse kruising.
    /// </summary>
    /// <param name="id">De identificator van de gelijkgrondse kruising.</param>
    /// <param name="parameters"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als de gelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als de gelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeToGradeSeparatedJunctionRoute, Name = nameof(ChangeGradeJunctionToGradeSeparatedJunctionV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeToGradeSeparatedJunctionV2Parameters), typeof(ChangeToGradeSeparatedJunctionV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeGradeJunctionToGradeSeparatedJunctionV2), Description = "Wijzig een gelijkgrondse kruising naar een ongelijkgrondse kruising.")]
    public async Task<IActionResult> ChangeGradeJunctionToGradeSeparatedJunctionV2(
        [FromRoute] int id,
        [FromBody] ChangeToGradeSeparatedJunctionV2Parameters parameters,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            if (!GradeJunctionId.Accepts(id))
            {
                throw new ValidationException([new ValidationFailure("id", $"De waarde {id} is ongeldig.")]);
            }

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. Whether the named road segments are this crossing's, and whether they differ, is left to
            // the domain: it is the only place that knows what the junction is about.
            var gradeJunction = await session.LoadAsync<GradeJunctionReadItem>(id, cancellationToken);
            if (gradeJunction is null)
            {
                return NotFound();
            }

            if (gradeJunction.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var (lowerRoadSegmentId, upperRoadSegmentId, type) = TranslateAndValidate(parameters);

            var sqsRequest = new ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                GradeJunctionId = new GradeJunctionId(id),
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

    private static (RoadSegmentId Lower, RoadSegmentId Upper, GradeSeparatedJunctionTypeV2 Type) TranslateAndValidate(ChangeToGradeSeparatedJunctionV2Parameters? parameters)
    {
        var failures = new List<ValidationFailure>();

        // VAL-4, VAL-6, VAL-9
        var lower = ParseRoadSegmentId(parameters?.OnderliggendWegsegment, "onderliggendWegsegment", failures);
        var upper = ParseRoadSegmentId(parameters?.BovenliggendWegsegment, "bovenliggendWegsegment", failures);
        var type = ParseType(parameters?.OngelijkgrondseKruisingType, failures);

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return (lower!.Value, upper!.Value, type!);
    }

    private static RoadSegmentId? ParseRoadSegmentId(string? value, string parameterName, List<ValidationFailure> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(new ValidationFailure(parameterName, $"De parameter '{parameterName}' is verplicht."));
            return null;
        }

        if (!int.TryParse(value, out var parsed) || !RoadSegmentId.Accepts(parsed))
        {
            failures.Add(new ValidationFailure(parameterName, $"De parameter '{parameterName}' heeft een ongeldige waarde."));
            return null;
        }

        return new RoadSegmentId(parsed);
    }

    // VAL-10: only the two kinds of grade separated crossing a caller may record.
    private static GradeSeparatedJunctionTypeV2? ParseType(string? value, List<ValidationFailure> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(new ValidationFailure("ongelijkgrondseKruisingType", "De parameter 'ongelijkgrondseKruisingType' is verplicht."));
            return null;
        }

        var type = ChangeableGradeSeparatedJunctionTypes
            .FirstOrDefault(x => string.Equals(x.ToDutchString(), value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (type is null)
        {
            failures.Add(new ValidationFailure("ongelijkgrondseKruisingType", "De parameter 'ongelijkgrondseKruisingType' heeft een ongeldige waarde."));
            return null;
        }

        return type;
    }

    // 'nietGekend' exists for what was imported without a known kind; it is not something a caller may ask for.
    internal static readonly GradeSeparatedJunctionTypeV2[] ChangeableGradeSeparatedJunctionTypes =
    [
        GradeSeparatedJunctionTypeV2.Brug,
        GradeSeparatedJunctionTypeV2.Tunnel
    ];
}

/// <summary>
///     De attribuutwaarden van de ongelijkgrondse kruising die in de plaats komt van de gelijkgrondse kruising.
/// </summary>
[DataContract(Name = "GelijkgrondseKruisingV2NaarOngelijkgrondseKruisingWijzigen", Namespace = "")]
public record ChangeToGradeSeparatedJunctionV2Parameters
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

public class ChangeToGradeSeparatedJunctionV2ParametersExamples : IExamplesProvider<ChangeToGradeSeparatedJunctionV2Parameters>
{
    public ChangeToGradeSeparatedJunctionV2Parameters GetExamples()
    {
        return new ChangeToGradeSeparatedJunctionV2Parameters
        {
            OnderliggendWegsegment = "432756",
            BovenliggendWegsegment = "987567",
            OngelijkgrondseKruisingType = GradeSeparatedJunctionTypeV2.Tunnel.ToDutchString()
        };
    }
}
