namespace RoadRegistry.BackOffice.Api.V2.RoadNodes;

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
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadNodesController
{
    private const string ChangeAttributesRoute = "acties/wijzigen/attributen";

    // Everything the request names is loaded and changed as one unit of work, so the size of a single request is
    // bounded here rather than let downstream time out on it.
    private const int ChangeAttributesMaximumRoadNodeCount = 1000;

    /// <summary>
    ///     Wijzig attribuutwaarde(n) voor één of meerdere wegknopen.
    /// </summary>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeAttributesRoute, Name = nameof(ChangeRoadNodeAttributesV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeRoadNodeAttributesV2Parameters), typeof(ChangeRoadNodeAttributesV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeRoadNodeAttributesV2), Description = "Wijzig één of meerdere attribuutwaarden voor één of meerdere wegknopen.")]
    public async Task<IActionResult> ChangeRoadNodeAttributesV2(
        [FromBody] ChangeRoadNodeAttributesV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1: the body must be a non-empty array of change objects.
            if (parameters is null || parameters.Count == 0)
            {
                throw new ValidationException([new ValidationFailure(nameof(parameters), "Ongeldige JSON.")]);
            }

            // Only the shape of the request is validated here. Whether the road nodes exist and are not removed is
            // validated by the domain, which is the only place that knows them.
            var groups = TranslateAndValidate(parameters);

            var sqsRequest = new ChangeRoadNodeAttributesV2SqsRequest
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

    private static IReadOnlyList<ChangeRoadNodeAttributesV2Group> TranslateAndValidate(ChangeRoadNodeAttributesV2Parameters parameters)
    {
        var failures = new List<ValidationFailure>();

        // VAL-7: a request that is too big is rejected on its own - reporting it together with the per-item failures
        // of a thousands-of-nodes request would bury it.
        var uniqueRoadNodeIdCount = parameters
            .Where(x => x.Wegknopen is not null)
            .SelectMany(x => x.Wegknopen!)
            .Distinct()
            .Count();
        if (uniqueRoadNodeIdCount > ChangeAttributesMaximumRoadNodeCount)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(parameters), $"Het maximaal toegestane aantal objecten waarvoor deze aanpassing mag gebeuren ({ChangeAttributesMaximumRoadNodeCount}) werd overschreden.")
            ]);
        }

        // VAL-2: the same attribute may only be given once per road node, across the whole request.
        var seenAttributePerRoadNode = new HashSet<(string Attribute, int RoadNodeId)>();

        var groups = new List<ChangeRoadNodeAttributesV2Group>();

        for (var i = 0; i < parameters.Count; i++)
        {
            var item = parameters[i];
            var path = $"[{i}]";

            // VAL-3
            if (item.Wegknopen is null || item.Wegknopen.Length == 0)
            {
                failures.Add(new ValidationFailure($"{path}.wegknopen", "De parameter 'wegknopen' is verplicht."));
                continue;
            }

            // VAL-3: 'grensknoop' says what the value must become, so the caller has to give it. Defaulting it to
            // false would silently unset a border node for anyone who left the parameter out.
            if (item.Grensknoop is null)
            {
                failures.Add(new ValidationFailure($"{path}.grensknoop", "De parameter 'grensknoop' is verplicht."));
                continue;
            }

            // VAL-4: an entry that is not an identifier the register could carry names no road node at all.
            var invalidIds = item.Wegknopen.Where(x => !RoadNodeId.Accepts(x)).ToArray();
            foreach (var invalidId in invalidIds)
            {
                failures.Add(new ValidationFailure($"{path}.wegknopen", $"De wegknoop {invalidId} bestaat niet."));
            }
            if (invalidIds.Length > 0)
            {
                continue;
            }

            // VAL-2
            foreach (var id in item.Wegknopen)
            {
                if (!seenAttributePerRoadNode.Add(("grensknoop", id)))
                {
                    failures.Add(new ValidationFailure(path, $"De parameter 'grensknoop' werd meermaals meegegeven voor wegknoop {id}."));
                }
            }

            groups.Add(new ChangeRoadNodeAttributesV2Group
            {
                RoadNodeIds = item.Wegknopen.Select(x => new RoadNodeId(x)).ToList(),
                Grensknoop = item.Grensknoop.Value
            });
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return groups;
    }
}

/// <summary>
///     Eén of meerdere groepen wegknopen samen met de attribuutwaarden die erop gewijzigd worden.
/// </summary>
[DataContract(Name = "WegknopenV2AttribuutwaardenWijzigen")]
public class ChangeRoadNodeAttributesV2Parameters : List<ChangeRoadNodeAttributeV2Parameters>
{
}

/// <summary>
///     Een groep wegknopen samen met de attribuutwaarden die erop gewijzigd worden.
/// </summary>
public record ChangeRoadNodeAttributeV2Parameters
{
    /// <summary>
    ///     Objectidentificatoren van de wegknopen waarop de wijziging van toepassing is.
    /// </summary>
    [DataMember(Name = "Wegknopen", Order = 0)]
    [JsonProperty("wegknopen", Required = Required.Always)]
    public int[]? Wegknopen { get; set; }

    /// <summary>
    ///     Attribuut dat aangeeft of een wegknoop aansluit op wegen die buiten het Vlaamse gewest liggen.
    /// </summary>
    [DataMember(Name = "Grensknoop", Order = 1)]
    [JsonProperty("grensknoop", Required = Required.Always)]
    public bool? Grensknoop { get; set; }
}

public class ChangeRoadNodeAttributesV2ParametersExamples : IExamplesProvider<ChangeRoadNodeAttributesV2Parameters>
{
    public ChangeRoadNodeAttributesV2Parameters GetExamples()
    {
        return
        [
            new ChangeRoadNodeAttributeV2Parameters
            {
                Wegknopen = [5642],
                Grensknoop = true
            }
        ];
    }
}
