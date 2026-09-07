namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

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
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Infrastructure;
using RoadRegistry.ValueObjects.ProblemCodes;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string DeleteRoadSegmentsRoute = "acties/verwijderen";

    // Everything the request names is removed as one unit of work, so the size of a single request is bounded here
    // rather than let downstream time out on it.
    private const int DeleteRoadSegmentsMaximumRoadSegmentCount = 1000;

    /// <summary>
    ///     Verwijder één of meerdere wegsegmenten.
    /// </summary>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(DeleteRoadSegmentsRoute, Name = nameof(DeleteRoadSegmentsV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(DeleteRoadSegmentsV2), Description = "Verwijder één of meerdere wegsegmenten. De wegknopen waar een verwijderd wegsegment aan hing worden aangepast of verwijderd, de kruisingen waar het deel van uitmaakte verdwijnen mee, en wegsegmenten die na de verwijdering nog enkel door een validatieknoop gescheiden worden, worden samengevoegd.")]
    public async Task<IActionResult> DeleteRoadSegmentsV2(
        [FromBody] DeleteRoadSegmentsV2Parameters parameters,
        [FromServices] DeleteRoadSegmentsV2ParametersValidator validator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Only the shape of the request is validated here. Whether the road segments exist is the domain's to
            // answer, because it is the only place that knows the network.
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            // Naming a segment twice asks for one removal, so the ceiling is counted over the distinct identifiers.
            // Reported on its own rather than alongside the per-identifier failures of a thousands-long request,
            // which would bury it.
            var roadSegmentIds = parameters.Wegsegmenten.Distinct().Select(x => new RoadSegmentId(x)).ToList();
            if (roadSegmentIds.Count > DeleteRoadSegmentsMaximumRoadSegmentCount)
            {
                throw new ValidationException([
                    new ValidationFailure(nameof(parameters.Wegsegmenten), $"Het maximaal toegestane aantal objecten waarvoor deze aanpassing mag gebeuren ({DeleteRoadSegmentsMaximumRoadSegmentCount}) werd overschreden.")
                ]);
            }

            var sqsRequest = new RemoveRoadSegmentsSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Delete),
                RoadSegmentIds = roadSegmentIds
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

[DataContract(Name = "WegsegmentenVerwijderen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentenVerwijderenV2")]
public class DeleteRoadSegmentsV2Parameters
{
    /// <summary>
    ///     Objectidentificatoren van de wegsegmenten die verwijderd moeten worden.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 1)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[] Wegsegmenten { get; set; }
}

public class DeleteRoadSegmentsV2ParametersValidator : AbstractValidator<DeleteRoadSegmentsV2Parameters>
{
    public DeleteRoadSegmentsV2ParametersValidator()
    {
        // VAL-1: the array is required and every identifier in it has to be one.
        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
            .WithProblemCode(ProblemCode.Common.JsonInvalid);
    }
}
