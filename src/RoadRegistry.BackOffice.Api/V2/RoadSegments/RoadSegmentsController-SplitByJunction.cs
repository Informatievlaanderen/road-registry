namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Infrastructure;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

public partial class RoadSegmentsController
{
    private const string SplitByJunctionRoute = "acties/knippenopkruising";

    /// <summary>
    ///     Knip wegsegmenten op een kruising.
    /// </summary>
    /// <param name="store"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als de wegsegmenten gevonden zijn.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(SplitByJunctionRoute, Name = nameof(SplitRoadSegmentsByJunctionV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(SplitRoadSegmentsByJunctionV2Parameters), typeof(SplitRoadSegmentsByJunctionV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(SplitRoadSegmentsByJunctionV2), Description = "Knip wegsegmenten op een kruising.")]
    public async Task<IActionResult> SplitRoadSegmentsByJunctionV2(
        [FromServices] IDocumentStore store,
        [FromBody] SplitRoadSegmentsByJunctionV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var session = store.LightweightSession();

            var (found1, status1) = await LoadRoadSegment(session, parameters?.Wegsegment1, cancellationToken);
            var (found2, status2) = await LoadRoadSegment(session, parameters?.Wegsegment2, cancellationToken);

            var request = new SplitRoadSegmentsByJunctionRequest
            {
                Wegsegment1 = parameters?.Wegsegment1,
                Wegsegment2 = parameters?.Wegsegment2,
                Wegsegment1Found = found1,
                Wegsegment2Found = found2,
                Wegsegment1Status = status1,
                Wegsegment2Status = status2
            };
            await new SplitRoadSegmentsByJunctionRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

            var sqsRequest = new SplitRoadSegmentsByJunctionV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                RoadSegmentId1 = new RoadSegmentId(parameters!.Wegsegment1!.Value),
                RoadSegmentId2 = new RoadSegmentId(parameters.Wegsegment2!.Value)
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }

    private static async Task<(bool Found, RoadSegmentStatusV2? Status)> LoadRoadSegment(IDocumentSession session, int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return (false, null);
        }

        var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id.Value, cancellationToken);
        if (roadSegment is null || roadSegment.IsRemoved)
        {
            return (false, null);
        }

        return (true, RoadSegmentStatusV2.Parse(roadSegment.Status));
    }

    private sealed record SplitRoadSegmentsByJunctionRequest
    {
        public required int? Wegsegment1 { get; init; }
        public required int? Wegsegment2 { get; init; }
        public required bool Wegsegment1Found { get; init; }
        public required bool Wegsegment2Found { get; init; }
        public RoadSegmentStatusV2? Wegsegment1Status { get; init; }
        public RoadSegmentStatusV2? Wegsegment2Status { get; init; }
    }

    private sealed class SplitRoadSegmentsByJunctionRequestValidator : AbstractValidator<SplitRoadSegmentsByJunctionRequest>
    {
        public SplitRoadSegmentsByJunctionRequestValidator()
        {
            // VAL-1: verplichte invoer.
            RuleFor(x => x.Wegsegment1)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.Wegsegment1IsRequired);
            RuleFor(x => x.Wegsegment2)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.Wegsegment2IsRequired);

            // VAL-2: must match an existing, non-removed road segment. VAL-3: that road segment must have status gerealiseerd.
            When(x => x.Wegsegment1 is not null, () =>
            {
                RuleFor(x => x.Wegsegment1Found)
                    .Must(found => found)
                    .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.RoadSegmentNotFound, (request, _) =>
                        new RoadSegmentsSplitByJunctionRoadSegmentNotFound(new RoadSegmentId(request.Wegsegment1!.Value)));

                When(x => x.Wegsegment1Found, () =>
                {
                    RuleFor(x => x.Wegsegment1Status)
                        .Must(status => status == RoadSegmentStatusV2.Gerealiseerd)
                        .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.StatusNotValid, (request, _) =>
                            new RoadSegmentsSplitByJunctionStatusNotValid(new RoadSegmentId(request.Wegsegment1!.Value)));
                });
            });

            When(x => x.Wegsegment2 is not null, () =>
            {
                RuleFor(x => x.Wegsegment2Found)
                    .Must(found => found)
                    .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.RoadSegmentNotFound, (request, _) =>
                        new RoadSegmentsSplitByJunctionRoadSegmentNotFound(new RoadSegmentId(request.Wegsegment2!.Value)));

                When(x => x.Wegsegment2Found, () =>
                {
                    RuleFor(x => x.Wegsegment2Status)
                        .Must(status => status == RoadSegmentStatusV2.Gerealiseerd)
                        .WithProblemCode(ProblemCode.RoadSegment.SplitByJunction.StatusNotValid, (request, _) =>
                            new RoadSegmentsSplitByJunctionStatusNotValid(new RoadSegmentId(request.Wegsegment2!.Value)));
                });
            });
        }
    }
}

[DataContract(Name = "WegsegmentenV2KnippenOpKruising", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentenV2KnippenOpKruising")]
public record SplitRoadSegmentsByJunctionV2Parameters
{
    /// <summary>
    ///     De objectidentificator van het eerste wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegment1", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public int? Wegsegment1 { get; set; }

    /// <summary>
    ///     De objectidentificator van het tweede wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegment2", Order = 2)]
    [JsonProperty(Required = Required.Always)]
    public int? Wegsegment2 { get; set; }
}

public class SplitRoadSegmentsByJunctionV2ParametersExamples : IExamplesProvider<SplitRoadSegmentsByJunctionV2Parameters>
{
    public SplitRoadSegmentsByJunctionV2Parameters GetExamples()
    {
        return new SplitRoadSegmentsByJunctionV2Parameters
        {
            Wegsegment1 = 481110,
            Wegsegment2 = 481111
        };
    }
}
