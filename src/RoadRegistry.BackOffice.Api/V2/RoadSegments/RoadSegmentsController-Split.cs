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
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extensions;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Infrastructure;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string SplitRoute = "{id}/acties/knippen";

    /// <summary>
    ///     Knip een wegsegment.
    /// </summary>
    /// <param name="idValidator"></param>
    /// <param name="store"></param>
    /// <param name="parameters"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(SplitRoute, Name = nameof(SplitRoadSegmentV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(SplitRoadSegmentV2Parameters), typeof(SplitRoadSegmentV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(SplitRoadSegmentV2), Description = "Knip een wegsegment op de opgegeven knippositie.")]
    public async Task<IActionResult> SplitRoadSegmentV2(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromServices] IDocumentStore store,
        [FromBody] SplitRoadSegmentV2Parameters parameters,
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            await using var session = store.LightweightSession();
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id, cancellationToken);
            if (roadSegment is null || roadSegment.IsRemoved)
            {
                throw new RoadSegmentNotFoundException();
            }

            var request = new SplitRoadSegmentV2Request
            {
                RoadSegmentId = new RoadSegmentId(id),
                Status = RoadSegmentStatusV2.Parse(roadSegment.Status),
                IsV2 = roadSegment.IsV2,
                RoadSegmentGeometry = roadSegment.Geometry.Lambert08.Value,
                Knippositie = parameters?.Knippositie
            };
            await new SplitRoadSegmentV2RequestValidator().ValidateAndThrowAsync(request, cancellationToken);

            var cutPosition = GeometryTranslator.ParseGmlPoint(request.Knippositie);

            var sqsRequest = new SplitRoadSegmentV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                RoadSegmentId = new RoadSegmentId(id),
                CutPosition = cutPosition
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }

    private sealed record SplitRoadSegmentV2Request
    {
        public required RoadSegmentId RoadSegmentId { get; init; }
        public required RoadSegmentStatusV2 Status { get; init; }
        public required bool IsV2 { get; init; }
        public required MultiLineString RoadSegmentGeometry { get; init; }
        public required string Knippositie { get; init; }
    }

    private sealed class SplitRoadSegmentV2RequestValidator : AbstractValidator<SplitRoadSegmentV2Request>
    {
        private static readonly RoadSegmentStatusV2[] ValidSplitStatuses =
        [
            RoadSegmentStatusV2.Gepland,
            RoadSegmentStatusV2.Gerealiseerd,
            RoadSegmentStatusV2.BuitenGebruik
        ];

        public SplitRoadSegmentV2RequestValidator()
        {
            // The road segment must have inwinningsstatus 'compleet' (i.e. be a V2 road segment).
            RuleFor(x => x.IsV2)
                .Must(isV2 => isV2)
                .WithProblemCode(ProblemCode.RoadSegment.Split.NotCompletedInwinning, (request, _) => new RoadSegmentSplitNotCompletedInwinning(request.RoadSegmentId));

            // VAL-3: the road segment has status gepland, gerealiseerd or buiten gebruik
            RuleFor(x => x.Status)
                .Must(status => ValidSplitStatuses.Contains(status))
                .WithProblemCode(ProblemCode.RoadSegment.Split.StatusNotValid, (request, _) => new RoadSegmentSplitStatusNotValid(request.RoadSegmentId));

            // VAL-4/5/6: knippositie is required, a valid gml 3.2 point in Lambert 2008
            RuleFor(x => x.Knippositie)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithProblemCode(ProblemCode.RoadSegment.Split.PositionIsRequired)
                .Must(GeometryTranslator.GmlIsValidPoint)
                    .WithProblemCode(ProblemCode.RoadSegment.Split.PositionGeometryNotValid)
                .Must(gml => GeometryTranslator.ParseGmlPoint(gml).SRID == WellknownSrids.Lambert08)
                    .WithProblemCode(ProblemCode.RoadSegment.Split.PositionSridNotLambert08);

            // VAL-7: the perpendicular projection distance of the knippositie to the road segment is <= 1m
            When(x => IsValidLambert08Point(x.Knippositie), () =>
            {
                RuleFor(x => x)
                    .Must(IsWithinMaximumDistanceOfRoadSegment)
                    .WithProblemCode(ProblemCode.RoadSegment.Split.PositionTooFarFromRoadSegment,
                        (_, __) => new RoadSegmentSplitPositionTooFarFromRoadSegment(Distances.RoadSegmentSplitMaximumDistanceToRoadSegment));
            });
        }

        private static bool IsValidLambert08Point(string gml)
        {
            return GeometryTranslator.GmlIsValidPoint(gml)
                   && GeometryTranslator.ParseGmlPoint(gml).SRID == WellknownSrids.Lambert08;
        }

        private static bool IsWithinMaximumDistanceOfRoadSegment(SplitRoadSegmentV2Request request)
        {
            var cutPosition = GeometryTranslator.ParseGmlPoint(request.Knippositie);
            return request.RoadSegmentGeometry.Distance(cutPosition) <= Distances.RoadSegmentSplitMaximumDistanceToRoadSegment;
        }
    }
}

[DataContract(Name = "WegsegmentV2Knippen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2Knippen")]
public record SplitRoadSegmentV2Parameters
{
    /// <summary>
    ///     GML-puntgeometrie van de knippositie, in het coördinatenstelsel Lambert 2008 (EPSG:3812).
    /// </summary>
    [DataMember(Name = "Knippositie", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public string Knippositie { get; set; }
}

public class SplitRoadSegmentV2ParametersExamples : IExamplesProvider<SplitRoadSegmentV2Parameters>
{
    public SplitRoadSegmentV2Parameters GetExamples()
    {
        return new SplitRoadSegmentV2Parameters
        {
            Knippositie = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:pos>217368.75 181577.01</gml:pos>
</gml:Point>"
        };
    }
}
