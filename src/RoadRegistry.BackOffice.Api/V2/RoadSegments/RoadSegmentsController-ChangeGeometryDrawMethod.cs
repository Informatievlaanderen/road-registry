namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string ChangeGeometryDrawMethodRoute = "acties/wijzigen/geometriemethode";

    // Everything the request names is loaded and changed as one unit of work, so the size of a single request is
    // bounded here rather than let downstream time out on it.
    private const int ChangeGeometryDrawMethodMaximumRoadSegmentCount = 1000;

    /// <summary>
    ///     Wijzig de geometriemethode voor één of meerdere wegsegmenten.
    /// </summary>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeGeometryDrawMethodRoute, Name = nameof(ChangeRoadSegmentGeometryDrawMethodV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeRoadSegmentGeometryDrawMethodV2Parameters), typeof(ChangeRoadSegmentGeometryDrawMethodV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeRoadSegmentGeometryDrawMethodV2), Description = "Wijzig de geometriemethode voor één of meerdere wegsegmenten.")]
    public async Task<IActionResult> ChangeRoadSegmentGeometryDrawMethodV2(
        [FromBody] ChangeRoadSegmentGeometryDrawMethodV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1: the body must be a non-empty array of change objects.
            if (parameters is null || parameters.Count == 0)
            {
                throw new ValidationException([new ValidationFailure(nameof(parameters), "Ongeldige JSON.")]);
            }

            // Only the shape of the request is validated here. Everything content related (does the road segment
            // exist, does it have an editable status) is validated by the domain.
            var groups = TranslateAndValidateGeometryDrawMethodChange(parameters);

            var sqsRequest = new ChangeRoadSegmentGeometryDrawMethodV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                Groups = groups
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }

    private static IReadOnlyList<ChangeRoadSegmentGeometryDrawMethodV2Group> TranslateAndValidateGeometryDrawMethodChange(
        ChangeRoadSegmentGeometryDrawMethodV2Parameters parameters)
    {
        var failures = new List<ValidationFailure>();

        // VAL-9: a request that is too big is rejected on its own: reporting it together with the per-item failures
        // of a thousands-of-segments request would bury it.
        var uniqueRoadSegmentIdCount = parameters
            .Where(x => x.Wegsegmenten is not null)
            .SelectMany(x => x.Wegsegmenten!)
            .Distinct()
            .Count();
        if (uniqueRoadSegmentIdCount > ChangeGeometryDrawMethodMaximumRoadSegmentCount)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(parameters), $"Er kunnen maximaal {ChangeGeometryDrawMethodMaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er {uniqueRoadSegmentIdCount} opgegeven.")
            ]);
        }

        // VAL-2: the parameter may only be specified once per road segment (across the whole request).
        var seenRoadSegmentIds = new HashSet<int>();

        var groups = new List<ChangeRoadSegmentGeometryDrawMethodV2Group>();

        for (var i = 0; i < parameters.Count; i++)
        {
            var item = parameters[i];
            var path = $"[{i}]";

            // VAL-3: wegsegmenten is required.
            if (item.Wegsegmenten is null || item.Wegsegmenten.Length == 0)
            {
                failures.Add(new ValidationFailure($"{path}.wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
                continue;
            }

            // VAL-2
            foreach (var id in item.Wegsegmenten)
            {
                if (!seenRoadSegmentIds.Add(id))
                {
                    failures.Add(new ValidationFailure(path, $"De parameter 'geometriemethode' werd meermaals meegegeven voor wegsegment {id}."));
                }
            }

            // VAL-7: geometriemethode is required.
            if (string.IsNullOrEmpty(item.Geometriemethode))
            {
                failures.Add(new ValidationFailure($"{path}.geometriemethode", "De parameter 'geometriemethode' is verplicht."));
                continue;
            }

            // VAL-8
            if (!RoadSegmentGeometryDrawMethodV2.CanParseUsingDutchName(item.Geometriemethode))
            {
                failures.Add(new ValidationFailure($"{path}.geometriemethode", "De parameter 'geometriemethode' heeft een ongeldige waarde."));
                continue;
            }

            groups.Add(new ChangeRoadSegmentGeometryDrawMethodV2Group
            {
                RoadSegmentIds = item.Wegsegmenten.Select(x => new RoadSegmentId(x)).ToList(),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.ParseUsingDutchName(item.Geometriemethode)
            });
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return groups;
    }
}

[DataContract(Name = "WegsegmentenV2GeometriemethodeWijzigen")]
public class ChangeRoadSegmentGeometryDrawMethodV2Parameters : List<ChangeRoadSegmentGeometryDrawMethodV2GroupParameters>
{
}

public record ChangeRoadSegmentGeometryDrawMethodV2GroupParameters
{
    /// <summary>
    ///     Objectidentificatoren van de wegsegmenten waarop de wijziging van toepassing is.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 0)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[]? Wegsegmenten { get; set; }

    /// <summary>
    ///     Aanduiding van de kwaliteit/betrouwbaarheid van de geometrie van het wegsegment aan de hand van de methode
    ///     gebruikt om deze geometrie te bepalen.
    /// </summary>
    [DataMember(Name = "Geometriemethode", Order = 1)]
    [JsonProperty("geometriemethode", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentGeometryDrawMethodV2))]
    public string Geometriemethode { get; set; }
}

public class ChangeRoadSegmentGeometryDrawMethodV2ParametersExamples : IExamplesProvider<ChangeRoadSegmentGeometryDrawMethodV2Parameters>
{
    public ChangeRoadSegmentGeometryDrawMethodV2Parameters GetExamples()
    {
        return
        [
            new ChangeRoadSegmentGeometryDrawMethodV2GroupParameters
            {
                Wegsegmenten = [481110, 481111],
                Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingemeten.ToDutchString()
            },
            new ChangeRoadSegmentGeometryDrawMethodV2GroupParameters
            {
                Wegsegmenten = [481112],
                Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingeschetst.ToDutchString()
            }
        ];
    }
}
