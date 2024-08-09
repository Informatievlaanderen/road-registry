namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegmentsOutline;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Controllers.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string CreateOutlineRoute = "acties/schetsen";

    /// <summary>
    ///     Schets een wegsegment.
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(CreateOutlineRoute, Name = nameof(CreateOutline))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostRoadSegmentOutlineParameters), typeof(PostRoadSegmentOutlineParametersExamples))]
    [SwaggerOperation(OperationId = nameof(CreateOutline), Description = "Voeg een nieuw wegsegment toe aan het Wegenregister met geometriemethode <ingeschetst>.")]
    public async Task<IActionResult> CreateOutline(
        [FromServices] PostRoadSegmentOutlineParametersValidator validator,
        [FromBody] PostRoadSegmentOutlineParameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var sqsRequest = new CreateRoadSegmentOutlineSqsRequest
            {
                Request = new CreateRoadSegmentOutlineRequest(
                    GeometryTranslator.Translate(GeometryTranslator.ParseGmlLineString(parameters.MiddellijnGeometrie)),
                    RoadSegmentStatus.ParseUsingDutchName(parameters.Wegsegmentstatus),
                    RoadSegmentMorphology.ParseUsingDutchName(parameters.MorfologischeWegklasse),
                    RoadSegmentAccessRestriction.ParseUsingDutchName(parameters.Toegangsbeperking),
                    new OrganizationId(parameters.Wegbeheerder),
                    RoadSegmentSurfaceType.ParseUsingDutchName(parameters.Wegverharding),
                    RoadSegmentWidth.ParseUsingDutchName(parameters.Wegbreedte),
                    RoadSegmentLaneCount.ParseUsingDutchName(parameters.AantalRijstroken.Aantal),
                    RoadSegmentLaneDirection.ParseUsingDutchName(parameters.AantalRijstroken.Richting)
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

[DataContract(Name = "WegsegmentSchetsen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentSchetsen")]
public record PostRoadSegmentOutlineParameters
{
    /// <summary>
    ///     De geometrie die de middellijn van het wegsegment vertegenwoordigt, het formaat gml 3.2 (linestring) en
    ///     coördinatenstelsel Lambert 72 (EPSG:31370).
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public string MiddellijnGeometrie { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 2)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatus.Edit))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     De wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 3)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphology.Edit))]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegankelijkheid van het wegsegment voor de weggebruiker.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 4)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestriction))]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De organisatie die verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 5)]
    [JsonProperty(Required = Required.Always)]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     Type wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 6)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceType))]
    public string Wegverharding { get; set; }

    /// <summary>
    ///     Breedte van het wegsegment in meter (geheel getal tussen 1 en 50 of "niet gekend" of "niet van toepassing").
    /// </summary>
    [DataMember(Name = "Wegbreedte", Order = 7)]
    [JsonProperty(Required = Required.Always)]
    public string Wegbreedte { get; set; }

    [DataMember(Name = "AantalRijstroken", Order = 8)]
    [JsonProperty(Required = Required.Always)]
    public RoadSegmentLaneParameters AantalRijstroken { get; set; }
}

/// <summary>
///     Aantal rijstroken van het wegsegment, en hun richting t.o.v. de richting van het wegsegment (begin- naar
///     eindknoop).
/// </summary>
public class RoadSegmentLaneParameters
{
    /// <summary>Aantal rijstroken van de wegsegmentschets (geheel getal tussen 1 en 10 of "niet gekend" of "niet van toepassing").</summary>
    [DataMember(Name = "Aantal", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public string Aantal { get; set; }

    /// <summary>De richting van deze rijstroken t.o.v. de richting van het wegsegment (begin- naar eindknoop).</summary>
    [DataMember(Name = "Richting", Order = 2)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentLaneDirection))]
    public string Richting { get; set; }
}

public class PostRoadSegmentOutlineParametersValidator : AbstractValidator<PostRoadSegmentOutlineParameters>
{
    private readonly IOrganizationCache _organizationCache;

    public PostRoadSegmentOutlineParametersValidator(IOrganizationCache organizationCache)
    {
        _organizationCache = organizationCache.ThrowIfNull();

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
            //TODO-rik test
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length < Distances.TooLongSegmentLength)
            .WithProblemCode(RoadSegmentGeometryLengthIsTooLong.ProblemCode, _ => new RoadSegmentGeometryLengthIsTooLong(Distances.TooLongSegmentLength));

        RuleFor(x => x.Wegsegmentstatus)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
            .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);

        RuleFor(x => x.MorfologischeWegklasse)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
            .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);

        RuleFor(x => x.Toegangsbeperking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
            .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);

        RuleFor(x => x.Wegbeheerder)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
            .Must(OrganizationId.AcceptsValue)
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid)
            .MustAsync(BeKnownOrganization)
            .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown, value => new MaintenanceAuthorityNotKnown(new OrganizationId(value)));

        RuleFor(x => x.Wegverharding)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.IsRequired)
            .Must(RoadSegmentSurfaceType.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.NotValid);

        RuleFor(x => x.Wegbreedte)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Width.IsRequired)
            .Must(x => RoadSegmentWidth.CanParseUsingDutchName(x) && RoadSegmentWidth.ParseUsingDutchName(x).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);

        RuleFor(x => x.AantalRijstroken)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
            .SetValidator(new RoadSegmentLaneParametersValidator());
    }

    private async Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        if (!OrganizationId.AcceptsValue(code))
        {
            return false;
        }

        var organization = await _organizationCache.FindByIdOrOvoCodeAsync(new OrganizationId(code), cancellationToken);
        return organization is not null;
    }
}

public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
{
    public RoadSegmentLaneParametersValidator()
    {
        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.LaneCount.IsRequired)
            .Must(x => RoadSegmentLaneCount.CanParseUsingDutchName(x) && RoadSegmentLaneCount.ParseUsingDutchName(x).IsValidForEdit())
            .WithProblemCode(ProblemCode.RoadSegment.LaneCount.NotValid);

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.IsRequired)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.NotValid);
    }
}

public class PostRoadSegmentOutlineParametersExamples : IExamplesProvider<PostRoadSegmentOutlineParameters>
{
    public PostRoadSegmentOutlineParameters GetExamples()
    {
        return new PostRoadSegmentOutlineParameters
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:LineString>",
            Wegsegmentstatus = RoadSegmentStatus.InUse.Translation.Name,
            MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.Translation.Name,
            Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.Translation.Name,
            Wegbeheerder = "44021",
            Wegverharding = RoadSegmentSurfaceType.SolidSurface.Translation.Name,
            Wegbreedte = "5",
            AantalRijstroken = new RoadSegmentLaneParameters
            {
                Aantal = "2",
                Richting = RoadSegmentLaneDirection.Forward.Translation.Name
            }
        };
    }
}
