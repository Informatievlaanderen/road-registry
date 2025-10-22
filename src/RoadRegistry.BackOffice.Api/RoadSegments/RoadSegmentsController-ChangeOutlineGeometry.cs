namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extensions;
using Abstractions.RoadSegmentsOutline;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ValidationException = FluentValidation.ValidationException;

public partial class RoadSegmentsController
{
    private const string ChangeOutlineGeometryRoute = "{id}/acties/wijzigen/schetsgeometrie";

    /// <summary>
    ///     Wijzig de geometrie van een ingeschetst wegsegment.
    /// </summary>
    /// <param name="idValidator"></param>
    /// <param name="validator"></param>
    /// <param name="roadSegmentRepository"></param>
    /// <param name="parameters"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeOutlineGeometryRoute, Name = nameof(ChangeOutlineGeometry))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostChangeOutlineGeometryParameters), typeof(PostChangeOutlineGeometryParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeOutlineGeometry), Description = "Wijzig de geometrie van een wegsegment met geometriemethode <ingeschetst>.")]
    public async Task<IActionResult> ChangeOutlineGeometry(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromServices] PostChangeOutlineGeometryParametersValidator validator,
        [FromServices] IRoadSegmentRepository roadSegmentRepository,
        [FromBody] PostChangeOutlineGeometryParameters parameters,
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            var roadSegment = await roadSegmentRepository.FindAsync(new RoadSegmentId(id), cancellationToken);
            if (roadSegment is null)
            {
                throw new RoadSegmentNotFoundException();
            }

            if (roadSegment.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            {
                var problemTranslation = new RoadSegmentGeometryDrawMethodNotOutlined().TranslateToDutch();
                throw new ValidationException(new[] {
                    new ValidationFailure
                    {
                        PropertyName = "objectId",
                        ErrorCode = problemTranslation.Code,
                        ErrorMessage = problemTranslation.Message
                    }
                });
            }

            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var sqsRequest = new ChangeRoadSegmentOutlineGeometrySqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                Request = new ChangeRoadSegmentOutlineGeometryRequest(
                    new RoadSegmentId(id),
                    GeometryTranslator.Translate(GeometryTranslator.ParseGmlLineString(parameters.MiddellijnGeometrie))
                )
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

[DataContract(Name = "WegsegmentSchetsGeometrieWijzigen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentSchetsGeometrieWijzigen")]
public record PostChangeOutlineGeometryParameters
{
    /// <summary>
    ///     De geometrie die de middellijn van het wegsegment vertegenwoordigt, het formaat gml 3.2 (linestring) en
    ///     coördinatenstelsel Lambert 72 (EPSG:31370).
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public string MiddellijnGeometrie { get; set; }
}

public class PostChangeOutlineGeometryParametersValidator : AbstractValidator<PostChangeOutlineGeometryParameters>
{
    public PostChangeOutlineGeometryParametersValidator()
    {
        RuleFor(x => x.MiddellijnGeometrie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.IsRequired)
            .Must(GeometryTranslator.GmlIsValidLineString)
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.NotValid)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).SRID == SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32())
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.SridNotValid)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length >= Distances.TooClose)
            .WithProblemCode(RoadSegmentGeometryLengthIsLessThanMinimum.ProblemCode, _ => new RoadSegmentGeometryLengthIsLessThanMinimum(Distances.TooClose))
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length < Distances.TooLongSegmentLength)
            .WithProblemCode(RoadSegmentGeometryLengthIsTooLong.ProblemCode, _ => new RoadSegmentGeometryLengthIsTooLong(Distances.TooLongSegmentLength));
    }
}

public class PostChangeOutlineGeometryParametersExamples : IExamplesProvider<PostChangeOutlineGeometryParameters>
{
    public PostChangeOutlineGeometryParameters GetExamples()
    {
        return new PostChangeOutlineGeometryParameters
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:LineString>"
        };
    }
}
