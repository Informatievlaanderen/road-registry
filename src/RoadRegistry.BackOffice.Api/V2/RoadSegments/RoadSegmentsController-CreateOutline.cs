namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Handlers.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;

public partial class RoadSegmentsController
{
    private const string CreateOutlineRoute = "acties/schetsen";

    /// <summary>
    ///     Schets een wegsegment.
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="roadNetworkIdGenerator"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(CreateOutlineRoute, Name = nameof(CreateOutlinedRoadSegmentV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(CreateOutlinedRoadSegmentV2Parameters), typeof(CreateOutlinedRoadSegmentV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(CreateOutlinedRoadSegmentV2), Description = "Voeg een nieuw wegsegment toe aan het Wegenregister met geometriemethode <ingeschetst>.")]
    public async Task<IActionResult> CreateOutlinedRoadSegmentV2(
        [FromServices] CreateOutlinedRoadSegmentV2ParametersValidator validator,
        [FromServices] IRoadNetworkIdGenerator roadNetworkIdGenerator,
        [FromBody] CreateOutlinedRoadSegmentV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var parsedGeometry = GeometryTranslator.ParseGmlLineString(parameters.WegsegmentGeometrie);
            var geometryLength = parsedGeometry.Length;

            // A missing vanPositie means the start of the segment and a missing totPositie its end. A totPositie of 0
            // still means the end as well, as it did before null was accepted.
            RoadSegmentPositionV2 ResolveFromPosition(double? vanPositie) =>
                new(vanPositie ?? 0);
            RoadSegmentPositionV2 ResolveToPosition(double? totPositie) =>
                new(totPositie is null || totPositie.Value.RoundToCm().Equals(0.0) ? geometryLength : totPositie.Value);

            var sqsRequest = new CreateRoadSegmentOutlineV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                RoadSegmentId = await roadNetworkIdGenerator.NewRoadSegmentIdAsync(),
                Geometry = parsedGeometry.ToRoadSegmentGeometry(),
                Status = RoadSegmentStatusV2.ParseUsingDutchName(parameters.Wegsegmentstatus),
                Morphology = parameters.Morfologie
                    .Select(x => new ChangeRoadSegmentMorphologyAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        Morphology = RoadSegmentMorphologyV2.ParseUsingDutchName(x.Morfologie)
                    })
                    .ToArray(),
                SurfaceType = parameters.Wegverharding
                    .Select(x => new ChangeRoadSegmentSurfaceTypeAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        SurfaceType = RoadSegmentSurfaceTypeV2.ParseUsingDutchName(x.Wegverharding)
                    })
                    .ToArray(),
                AccessRestriction = parameters.Toegang
                    .Select(x => new ChangeRoadSegmentAccessRestrictionAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        AccessRestriction = RoadSegmentAccessRestrictionV2.ParseUsingDutchName(x.Toegang)
                    })
                    .ToArray(),
                StreetNameId = parameters.Straatnaam
                    .Select(x => new ChangeRoadSegmentStreetNameIdAttributeValue
                    {
                        Side = RoadSegmentAttributeSide.ParseUsingDutchName(x.Kant),
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        StreetNameId = ParseStreetNameLocalId(x.Identificator)
                    })
                    .ToArray(),
                MaintenanceAuthorityId = parameters.Wegbeheerder
                    .Select(x => new ChangeRoadSegmentMaintenanceAuthorityIdAttributeValue
                    {
                        Side = RoadSegmentAttributeSide.ParseUsingDutchName(x.Kant),
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        MaintenanceAuthorityId = new OrganizationId(x.Wegbeheerder)
                    })
                    .ToArray(),
                Category = parameters.Wegcategorie
                    .Select(x => new ChangeRoadSegmentCategoryAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        Category = RoadSegmentCategoryV2.ParseUsingDutchName(x.Wegcategorie)
                    })
                    .ToArray(),
                CarTrafficDirection = parameters.VerkeerstypeAuto
                    .Select(x => new ChangeRoadSegmentCarTrafficDirectionAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        TrafficDirection = RoadSegmentTrafficDirection.ParseUsingDutchName(x.Richting)
                    })
                    .ToArray(),
                BikeTrafficDirection = parameters.VerkeerstypeFiets
                    .Select(x => new ChangeRoadSegmentBikeTrafficDirectionAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        TrafficDirection = RoadSegmentTrafficDirection.ParseUsingDutchName(x.Richting)
                    })
                    .ToArray(),
                PedestrianTrafficDirection = parameters.VerkeerstypeVoetganger
                    .Select(x => new ChangeRoadSegmentPedestrianTrafficDirectionAttributeValue
                    {
                        FromPosition = ResolveFromPosition(x.VanPositie),
                        ToPosition = ResolveToPosition(x.TotPositie),
                        TrafficDirection = RoadSegmentPedestrianTrafficDirection.ParseUsingDutchName(x.Richting)
                    })
                    .ToArray(),
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }

    private static StreetNameLocalId ParseStreetNameLocalId(string identificator)
    {
        return StreetNameLocalId.TryParseUsingDutchName(identificator, out var streetNameLocalId) && streetNameLocalId == StreetNameLocalId.NotApplicable
            ? StreetNameLocalId.NotApplicable
            : new StreetNameLocalId(identificator.GetIdentifierFromPuri());
    }
}

/// <summary>
///     De gegevens van het in te schetsen wegsegment.
/// </summary>
[DataContract(Name = "WegsegmentV2Schetsen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2Schetsen")]
public record CreateOutlinedRoadSegmentV2Parameters
{
    /// <summary>
    ///     GML-lijngeometrie van het wegsegment, in het coördinatenstelsel Lambert 2008 (EPSG:3812).
    /// </summary>
    [DataMember(Name = "WegsegmentGeometrie", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public string WegsegmentGeometrie { get; set; }

    /// <summary>
    ///     Attribuut dat de levensloopfase van een wegsegment aangeeft.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 2)]
    [JsonProperty(Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatusV2.EditOutlined))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de vorm beschrijft die een weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "Morfologie", Order = 3)]
    [JsonProperty(Required = Required.Always)]
    public MorfologieParameters[] Morfologie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft welk type verharding van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 4)]
    [JsonProperty(Required = Required.Always)]
    public WegverhardingParameters[] Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke mate een weg toegankelijk is voor weggebruikers in het algemeen, ongeacht het type weggebruiker (voetgangers, fietsers, etc.).
    /// </summary>
    [DataMember(Name = "Toegang", Order = 5)]
    [JsonProperty(Required = Required.Always)]
    public ToegangParameters[] Toegang { get; set; }

    /// <summary>
    ///     De straatnaam uit het Adressenregister gekoppeld aan het wegsegment.
    /// </summary>
    [DataMember(Name = "Straatnaam", Order = 6)]
    [JsonProperty(Required = Required.Always)]
    public IngeschetstWegsegmentStraatnaamAttribuutWaarde[] Straatnaam { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft wie verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 7)]
    [JsonProperty(Required = Required.Always)]
    public IngeschetstWegsegmentWegbeheerderAttribuutWaarde[] Wegbeheerder { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de categorie weergeeft van een weg zoals vastgelegd door de Vlaamse Overheid.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 8)]
    [JsonProperty(Required = Required.Always)]
    public WegcategorieParameters[] Wegcategorie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor auto’s.
    /// </summary>
    [DataMember(Name = "VerkeerstypeAuto", Order = 9)]
    [JsonProperty(Required = Required.Always)]
    public VerkeerstypeParameters[] VerkeerstypeAuto { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor fietsers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeFiets", Order = 10)]
    [JsonProperty(Required = Required.Always)]
    public VerkeerstypeParameters[] VerkeerstypeFiets { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft of het wegsegment toegankelijk is voor voetgangers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeVoetganger", Order = 11)]
    [JsonProperty(Required = Required.Always)]
    public VerkeerstypeVoetgangerParameters[] VerkeerstypeVoetganger { get; set; }
}

// Like the attribute values of the change attributes and change geometry endpoints, vanPositie and totPositie may be
// left out (or null) while the value itself is required, so a request missing it is refused as invalid JSON. Unlike
// there, kant is required as well.

/// <summary>
///     De straatnaam die geldt voor een bepaald deel en een bepaalde kant van het ingeschetste wegsegment.
/// </summary>
[CustomSwaggerSchemaId("IngeschetstWegsegmentStraatnaamAttribuutWaarde")]
public record IngeschetstWegsegmentStraatnaamAttribuutWaarde : VanTotParameters
{
    /// <summary>
    ///     Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    /// <summary>
    ///     Identificator van de straatnaam uit het Adressenregister, of 'niet van toepassing'.
    /// </summary>
    [DataMember(Name = "identificator", Order = 3)]
    [JsonProperty("identificator", Required = Required.Always)]
    public string Identificator { get; set; }
}

/// <summary>
///     De wegbeheerder die geldt voor een bepaald deel en een bepaalde kant van het ingeschetste wegsegment.
/// </summary>
[CustomSwaggerSchemaId("IngeschetstWegsegmentWegbeheerderAttribuutWaarde")]
public record IngeschetstWegsegmentWegbeheerderAttribuutWaarde : VanTotParameters
{
    /// <summary>
    ///     Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    /// <summary>
    ///     Organisatiecode van de wegbeheerder.
    /// </summary>
    [DataMember(Name = "wegbeheerder", Order = 3)]
    [JsonProperty("wegbeheerder", Required = Required.Always)]
    public string Wegbeheerder { get; set; }
}

public class CreateOutlinedRoadSegmentV2ParametersValidator : AbstractValidator<CreateOutlinedRoadSegmentV2Parameters>
{
    private readonly IOrganizationCache _organizationCache;

    public CreateOutlinedRoadSegmentV2ParametersValidator(IOrganizationCache organizationCache)
    {
        _organizationCache = organizationCache.ThrowIfNull();

        RuleFor(x => x.WegsegmentGeometrie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Geometry.IsRequired)
            .Must(GeometryTranslator.GmlIsValidLineString)
                .WithProblemCode(ProblemCode.RoadSegment.Geometry.NotValid)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).SRID == WellknownSrids.Lambert08)
                .WithProblemCode(ProblemCode.RoadSegment.Geometry.SridNotLambert08)
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length >= Distances.RoadSegmentV2MinimumLength)
                .WithProblemCode(RoadSegmentGeometryLengthIsLessThanMinimum.ProblemCode, _ => new RoadSegmentGeometryLengthIsLessThanMinimum(Distances.RoadSegmentV2MinimumLength))
            .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length < Distances.TooLongSegmentLength)
                .WithProblemCode(RoadSegmentGeometryLengthIsTooLong.ProblemCode, _ => new RoadSegmentGeometryLengthIsTooLong(Distances.TooLongSegmentLength))
            .Must(gml => !HasVerticesTooClose(gml))
                .WithProblemCode(ProblemCode.RoadSegment.Geometry.VerticesTooClose)
            .Must(gml => !IsMorePreciseThanCm(gml))
                .WithProblemCode(ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm);

        RuleFor(x => x.Wegsegmentstatus)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
            .Must(v => RoadSegmentStatusV2.CanParseUsingDutchName(v) && RoadSegmentStatusV2.EditOutlined.Values.Contains(RoadSegmentStatusV2.ParseUsingDutchName(v)))
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);

        // Morfologie
        RuleFor(x => x.Morfologie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Morfologie is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Morfologie).ChildRules(item =>
            {
                item.RuleFor(x => x.Morfologie)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
                    .Must(RoadSegmentMorphologyV2.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Morfologie).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Morfologie),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes)));
        });

        // Wegverharding
        RuleFor(x => x.Wegverharding)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Wegverharding is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Wegverharding).ChildRules(item =>
            {
                item.RuleFor(x => x.Wegverharding)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.IsRequired)
                    .Must(RoadSegmentSurfaceTypeV2.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Wegverharding).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Wegverharding),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes)));
        });

        // Toegang
        RuleFor(x => x.Toegang)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Toegang is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Toegang).ChildRules(item =>
            {
                item.RuleFor(x => x.Toegang)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
                    .Must(RoadSegmentAccessRestrictionV2.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Toegang).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Toegang),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes)));
        });

        // Straatnaam
        RuleFor(x => x.Straatnaam)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.HasCountOfZero)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Straatnaam is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Straatnaam).ChildRules(item =>
            {
                item.RuleFor(x => x.Kant)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.AttributeSide.NotValid)
                    .Must(RoadSegmentAttributeSide.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.AttributeSide.NotValid);
                // The problem names the side the identificator was given for. With a kant that is missing or unknown
                // (reported on its own) the side is unknown, so the identificator is checked without one.
                foreach (var (side, notValidProblemCode) in new[]
                         {
                             (RoadSegmentAttributeSide.Links, ProblemCode.RoadSegment.StreetName.Left.NotValid),
                             (RoadSegmentAttributeSide.Rechts, ProblemCode.RoadSegment.StreetName.Right.NotValid),
                             ((RoadSegmentAttributeSide?)null, ProblemCode.RoadSegment.StreetName.NotValid)
                         })
                {
                    item.RuleFor(x => x.Identificator)
                        .Cascade(CascadeMode.Stop)
                        .NotEmpty()
                            .WithProblemCode(notValidProblemCode)
                        .MustBeValidStreetNameId(allowNotApplicable: true)
                            .WithProblemCode(notValidProblemCode)
                        .When(x => ParseSideOrNull(x.Kant) is var kant && (side is null
                            ? kant is null || kant == RoadSegmentAttributeSide.Beide
                            : kant == side));
                }
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Straatnaam).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.ValidateSided(
                    (items ?? []).Select(x => ((string?)x.Kant, x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Straatnaam),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes,
                    ProblemCode.RoadSegment.StreetName.NotOnBothSides)));
        });

        // Wegbeheerder
        RuleFor(x => x.Wegbeheerder)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Wegbeheerder is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Wegbeheerder).ChildRules(item =>
            {
                item.RuleFor(x => x.Kant)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.AttributeSide.NotValid)
                    .Must(RoadSegmentAttributeSide.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.AttributeSide.NotValid);
                item.RuleFor(x => x.Wegbeheerder)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
                    .Must(OrganizationId.AcceptsValue)
                        .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid)
                    .Must(BeKnownOrganization)
                        .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown,
                            v => new MaintenanceAuthorityNotKnown(new OrganizationId(v)));
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Wegbeheerder).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.ValidateSided(
                    (items ?? []).Select(x => ((string?)x.Kant, x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Wegbeheerder),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes,
                    ProblemCode.RoadSegment.MaintenanceAuthority.NotOnBothSides)));
        });

        // Wegcategorie
        RuleFor(x => x.Wegcategorie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Category.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.Wegcategorie is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.Wegcategorie).ChildRules(item =>
            {
                item.RuleFor(x => x.Wegcategorie)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.Category.IsRequired)
                    .Must(RoadSegmentCategoryV2.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.Wegcategorie).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.Wegcategorie),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes)));
        });

        // VerkeerstypeAuto
        RuleFor(x => x.VerkeerstypeAuto)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.CarTrafficDirection.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.CarTrafficDirection.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.VerkeerstypeAuto is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.VerkeerstypeAuto).ChildRules(item =>
            {
                item.RuleFor(x => x.Richting)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.CarTrafficDirection.IsRequired)
                    .Must(RoadSegmentTrafficDirection.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.CarTrafficDirection.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.VerkeerstypeAuto).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.VerkeerstypeAuto),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.CarTrafficDirection.DynamicAttributeProblemCodes)));
        });

        // VerkeerstypeFiets
        RuleFor(x => x.VerkeerstypeFiets)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.BikeTrafficDirection.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.BikeTrafficDirection.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.VerkeerstypeFiets is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.VerkeerstypeFiets).ChildRules(item =>
            {
                item.RuleFor(x => x.Richting)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.BikeTrafficDirection.IsRequired)
                    .Must(RoadSegmentTrafficDirection.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.BikeTrafficDirection.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.VerkeerstypeFiets).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.VerkeerstypeFiets),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.BikeTrafficDirection.DynamicAttributeProblemCodes)));
        });

        // VerkeerstypeVoetganger (only "beide" and "geen" allowed per RoadSegmentPedestrianTrafficDirection)
        RuleFor(x => x.VerkeerstypeVoetganger)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.PedestrianTrafficDirection.IsRequired)
            .Must(items => items.Length > 0)
                .WithProblemCode(ProblemCode.RoadSegment.PedestrianTrafficDirection.DynamicAttributeProblemCodes.HasCountOfZero);
        When(x => x.VerkeerstypeVoetganger is { Length: > 0 }, () =>
        {
            RuleForEach(x => x.VerkeerstypeVoetganger).ChildRules(item =>
            {
                item.RuleFor(x => x.Richting)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithProblemCode(ProblemCode.RoadSegment.PedestrianTrafficDirection.IsRequired)
                    .Must(RoadSegmentPedestrianTrafficDirection.CanParseUsingDutchName)
                        .WithProblemCode(ProblemCode.RoadSegment.PedestrianTrafficDirection.NotValid);
            });
        });
        When(IsGeometryValid, () =>
        {
            RuleFor(x => x.VerkeerstypeVoetganger).Custom((items, ctx) => AddFailures(ctx,
                RoadSegmentAttributePositionsValidator.Validate(
                    (items ?? []).Select(x => (x.VanPositie, ToPositie(x.TotPositie))),
                    nameof(CreateOutlinedRoadSegmentV2Parameters.VerkeerstypeVoetganger),
                    GetGeometryLength(ctx.InstanceToValidate),
                    ProblemCode.RoadSegment.PedestrianTrafficDirection.DynamicAttributeProblemCodes)));
        });
    }

    private static RoadSegmentAttributeSide? ParseSideOrNull(string? kant)
        => kant is not null && RoadSegmentAttributeSide.CanParseUsingDutchName(kant) ? RoadSegmentAttributeSide.ParseUsingDutchName(kant) : null;

    private static bool IsGeometryValid(CreateOutlinedRoadSegmentV2Parameters x)
        => x.WegsegmentGeometrie is not null && GeometryTranslator.GmlIsValidLineString(x.WegsegmentGeometrie);

    private static double GetGeometryLength(CreateOutlinedRoadSegmentV2Parameters x)
        => GeometryTranslator.ParseGmlLineString(x.WegsegmentGeometrie).Length.RoundToCm();

    private static bool HasVerticesTooClose(string gml)
    {
        return GeometryTranslator.ParseGmlLineString(gml)
            .GetSingleLineString()
            .ContainsVertexTooCloseToAnother(Distances.MinimumDistanceBetweenVertices);
    }

    private static bool IsMorePreciseThanCm(string gml)
    {
        return GeometryTranslator.ParseGmlLineString(gml)
            .GetSingleLineString()
            .HasCoordinatesMorePreciseThanCm();
    }

    // A missing totPositie means "up to the end of the geometry", and so does a totPositie of 0 - the only way to say
    // so before null was accepted.
    private static double? ToPositie(double? totPositie) => totPositie?.RoundToCm().Equals(0.0) == true ? null : totPositie;

    private static void AddFailures(
        ValidationContext<CreateOutlinedRoadSegmentV2Parameters> ctx,
        IEnumerable<ValidationFailure> failures)
    {
        foreach (var failure in failures)
        {
            ctx.AddFailure(failure);
        }
    }

    private bool BeKnownOrganization(string code)
    {
        if (!OrganizationId.AcceptsValue(code))
            return false;

        var organization = _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(code), CancellationToken.None).GetAwaiter().GetResult();
        return organization is not null;
    }
}

public class CreateOutlinedRoadSegmentV2ParametersExamples : IExamplesProvider<CreateOutlinedRoadSegmentV2Parameters>
{
    public CreateOutlinedRoadSegmentV2Parameters GetExamples()
    {
        var geometry = GeometryExtensions.WithSrid(new LineString([
            new(243234.8929999992, 160239.3830000013),
            new(243245.9949999973, 160238.7989999987),
            new(243261.3599999994, 160239.0),
            new(243279.0160000026, 160244.1570000015),
        ]), WellknownSrids.Lambert72);

        return new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = geometry.EnsureLambert08().ConvertToGml(useHttpsSchema: false, coordinatePrecision: Precisions.GeometryPrecisionV2),
            Wegsegmentstatus = RoadSegmentStatusV2.Gepland.ToDutchString(),
            Straatnaam = [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Links.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = 10,
                    Identificator = OsloNamespaces.StraatNaam.ToPuri(71671.ToString())
                },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Rechts.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = 10,
                    Identificator = OsloNamespaces.StraatNaam.ToPuri(65412.ToString())
                },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                    VanPositie = 10,
                    TotPositie = geometry.Length.RoundToCm(),
                    Identificator = OsloNamespaces.StraatNaam.ToPuri(71671.ToString())
                },
            ],
            Morfologie = new [] { RoadSegmentMorphologyV2.Parallelweg }
                .Select(x => new MorfologieParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Morfologie = x.ToDutchString()
                })
                .ToArray(),
            Toegang = new [] { RoadSegmentAccessRestrictionV2.OpenbareWeg }
                .Select(x => new ToegangParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Toegang = x.ToDutchString()
                })
                .ToArray(),
            Wegbeheerder = new [] { new OrganizationId("AGIV") }
                .Select(x => new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Wegbeheerder = x.ToString()
                })
                .ToArray(),
            Wegcategorie = new [] { RoadSegmentCategoryV2.RegionaleWeg }
                .Select(x => new WegcategorieParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Wegcategorie = x.ToDutchString()
                })
                .ToArray(),
            Wegverharding = new [] { RoadSegmentSurfaceTypeV2.Verhard }
                .Select(x => new WegverhardingParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Wegverharding = x.ToDutchString()
                })
                .ToArray(),
            VerkeerstypeAuto = new [] { RoadSegmentTrafficDirection.Forward }
                .Select(x => new VerkeerstypeParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Richting = x.ToDutchString()
                })
                .ToArray(),
            VerkeerstypeFiets = new [] { RoadSegmentTrafficDirection.Both }
                .Select(x => new VerkeerstypeParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Richting = x.ToDutchString()
                })
                .ToArray(),
            VerkeerstypeVoetganger = new [] { RoadSegmentTrafficDirection.None }
                .Select(x => new VerkeerstypeVoetgangerParameters
                {
                    VanPositie = 0,
                    TotPositie = geometry.Length.RoundToCm(),
                    Richting = x.ToDutchString()
                })
                .ToArray()
        };
    }
}
