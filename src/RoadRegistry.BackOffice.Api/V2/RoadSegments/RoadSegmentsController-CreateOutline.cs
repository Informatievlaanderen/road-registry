namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Handlers.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
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
    [HttpPost(CreateOutlineRoute, Name = nameof(CreateOutlinedRoadSegmentV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(CreateOutlinedRoadSegmentV2Parameters), typeof(CreateOutlinedRoadSegmentV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(CreateOutlinedRoadSegmentV2), Description = "Voeg een nieuw wegsegment toe aan het Wegenregister met geometriemethode <ingeschetst>.")]
    public async Task<IActionResult> CreateOutlinedRoadSegmentV2(
        [FromServices] PostRoadSegmentOutlineParametersValidator validator,
        [FromBody] CreateOutlinedRoadSegmentV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            //TODO-pr use v2 and new lambda
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var sqsRequest = new CreateRoadSegmentOutlineV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Geometry = GeometryTranslator.ParseGmlLineString(parameters.WegsegmentGeometrie).ToRoadSegmentGeometry(),
                Status = RoadSegmentStatusV2.ParseUsingDutchName(parameters.Wegsegmentstatus),
                Morphology = parameters.Morfologie
                    .Select(x => new ChangeRoadSegmentMorphologyAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        Morphology = RoadSegmentMorphologyV2.ParseUsingDutchName(x.Morfologie)
                    })
                    .ToArray(),
                SurfaceType = parameters.Wegverharding
                    .Select(x => new ChangeRoadSegmentSurfaceTypeAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        SurfaceType = RoadSegmentSurfaceTypeV2.ParseUsingDutchName(x.Wegverharding)
                    })
                    .ToArray(),
                AccessRestriction = parameters.Toegang
                    .Select(x => new ChangeRoadSegmentAccessRestrictionAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        AccessRestriction = RoadSegmentAccessRestrictionV2.ParseUsingDutchName(x.Toegang)
                    })
                    .ToArray(),
                StreetNameId = parameters.Straatnaam
                    .Select(x => new ChangeRoadSegmentStreetNameIdAttributeValue
                    {
                        Side = x.Kant.ToRoadSegmentAttributeSide(),
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        StreetNameId = new StreetNameLocalId(x.Identificator.GetIdentifierFromPuri())
                    })
                    .ToArray(),
                MaintenanceAuthorityId = parameters.Wegbeheerder
                    .Select(x => new ChangeRoadSegmentMaintenanceAuthorityIdAttributeValue
                    {
                        Side = x.Kant.ToRoadSegmentAttributeSide(),
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        MaintenanceAuthorityId = new OrganizationId(x.Wegbeheerder)
                    })
                    .ToArray(),
                Category = parameters.Wegcategorie
                    .Select(x => new ChangeRoadSegmentCategoryAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        Category = RoadSegmentCategoryV2.ParseUsingDutchName(x.Wegcategorie)
                    })
                    .ToArray(),
                CarTrafficDirection = parameters.VerkeerstypeAuto
                    .Select(x => new ChangeRoadSegmentCarTrafficDirectionAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        TrafficDirection = RoadSegmentTrafficDirection.ParseUsingDutchName(x.Richting)
                    })
                    .ToArray(),
                BikeTrafficDirection = parameters.VerkeerstypeFiets
                    .Select(x => new ChangeRoadSegmentBikeTrafficDirectionAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
                        TrafficDirection = RoadSegmentTrafficDirection.ParseUsingDutchName(x.Richting)
                    })
                    .ToArray(),
                PedestrianTrafficDirection = parameters.VerkeerstypeVoetganger
                    .Select(x => new ChangeRoadSegmentPedestrianTrafficDirectionAttributeValue
                    {
                        FromPosition = new RoadSegmentPositionV2(x.VanPositie),
                        ToPosition = new RoadSegmentPositionV2(x.TotPositie),
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
}

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
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatusV2.Edit))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de vorm beschrijft die een weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "Morfologie", Order = 3)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentMorfologieAttribuutWaarde[] Morfologie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft welk type verharding van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 4)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentWegverhardingAttribuutWaarde[] Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke mate een weg toegankelijk is voor weggebruikers in het algemeen, ongeacht het type weggebruiker (voetgangers, fietsers, etc.).
    /// </summary>
    [DataMember(Name = "Toegang", Order = 5)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentToegangAttribuutWaarde[] Toegang { get; set; }

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
    public WegsegmentWegcategorieAttribuutWaarde[] Wegcategorie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor auto’s.
    /// </summary>
    [DataMember(Name = "VerkeerstypeAuto", Order = 9)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentVerkeerstypeAutoAttribuutWaarde[] VerkeerstypeAuto { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor fietsers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeFiets", Order = 10)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentVerkeerstypeFietsAttribuutWaarde[] VerkeerstypeFiets { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft of het wegsegment toegankelijk is voor voetgangers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeVoetganger", Order = 11)]
    [JsonProperty(Required = Required.Always)]
    public WegsegmentVerkeerstypeVoetgangerAttribuutWaarde[] VerkeerstypeVoetganger { get; set; }
}

[DataContract(Name = "IngeschetstWegsegmentStraatnaamAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("IngeschetstWegsegmentStraatnaamAttribuutWaarde")]
public class IngeschetstWegsegmentStraatnaamAttribuutWaarde
{
    /// <summary>
    /// Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "Kant", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegsegmentKant Kant { get; set; }

    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    /// <summary>
    /// Identificator van de straatnaam.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Identificator { get; set; }
}

[DataContract(Name = "IngeschetstWegsegmentWegbeheerderAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("IngeschetstWegsegmentWegbeheerderAttribuutWaarde")]
public class IngeschetstWegsegmentWegbeheerderAttribuutWaarde
{
    /// <summary>
    /// Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "Kant", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegsegmentKant Kant { get; set; }

    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    /// <summary>
    /// Organisatiecode van de wegbeheerder.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Wegbeheerder { get; set; }
}

public class PostRoadSegmentOutlineParametersValidator : AbstractValidator<CreateOutlinedRoadSegmentV2Parameters>
{
    private readonly IOrganizationCache _organizationCache;

    public PostRoadSegmentOutlineParametersValidator(IOrganizationCache organizationCache)
    {
        _organizationCache = organizationCache.ThrowIfNull();

        //TODO-pr validations
        // RuleFor(x => x.WegsegmentGeometrie)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.Geometry.IsRequired)
        //     .Must(GeometryTranslator.GmlIsValidLineString)
        //     .WithProblemCode(ProblemCode.RoadSegment.Geometry.NotValid)
        //     .Must(gml => GeometryTranslator.ParseGmlLineString(gml).SRID == WellknownSrids.Lambert08)
        //     .WithProblemCode(ProblemCode.RoadSegment.Geometry.SridNotValid)
        //     .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length >= Distances.RoadSegmentV2MinimumLength)
        //     .WithProblemCode(RoadSegmentGeometryLengthIsLessThanMinimum.ProblemCode, _ => new RoadSegmentGeometryLengthIsLessThanMinimum(Distances.RoadSegmentV2MinimumLength))
        //     .Must(gml => GeometryTranslator.ParseGmlLineString(gml).Length < Distances.TooLongSegmentLength)
        //     .WithProblemCode(RoadSegmentGeometryLengthIsTooLong.ProblemCode, _ => new RoadSegmentGeometryLengthIsTooLong(Distances.TooLongSegmentLength));
        //
        // RuleFor(x => x.Wegsegmentstatus)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
        //     .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value).IsValidForEdit())
        //     .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        //
        // RuleFor(x => x.MorfologischeWegklasse)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
        //     .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value).IsValidForEdit())
        //     .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        //
        // RuleFor(x => x.Toegangsbeperking)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
        //     .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
        //     .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
        //
        // RuleFor(x => x.Wegbeheerder)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
        //     .Must(OrganizationId.AcceptsValue)
        //     .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid)
        //     .Must(BeKnownOrganization)
        //     .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown, value => new MaintenanceAuthorityNotKnown(new OrganizationId(value)));
        //
        // RuleFor(x => x.Wegverharding)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.IsRequired)
        //     .Must(RoadSegmentSurfaceType.CanParseUsingDutchName)
        //     .WithProblemCode(ProblemCode.RoadSegment.SurfaceType.NotValid);
        //
        // RuleFor(x => x.Wegbreedte)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.Width.IsRequired)
        //     .Must(x => RoadSegmentWidth.CanParseUsingDutchName(x) && RoadSegmentWidth.ParseUsingDutchName(x).IsValidForEdit())
        //     .WithProblemCode(ProblemCode.RoadSegment.Width.NotValid);
        //
        // RuleFor(x => x.AantalRijstroken)
        //     .Cascade(CascadeMode.Stop)
        //     .NotNull()
        //     .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
        //     .SetValidator(new RoadSegmentLaneParametersValidator());
        //
        // When(x => x.Wegcategorie is not null, () =>
        // {
        //     RuleFor(x => x.Wegcategorie)
        //         .Cascade(CascadeMode.Stop)
        //         .Must(x => RoadSegmentCategory.TryParseUsingDutchName(x, out var category) && category.IsValidForEdit())
        //         .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        // });
    }

    private bool BeKnownOrganization(string code)
    {
        if (!OrganizationId.AcceptsValue(code))
        {
            return false;
        }

        var organization = _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(code), CancellationToken.None).GetAwaiter().GetResult();
        return organization is not null;
    }
}

// public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
// {
//     public RoadSegmentLaneParametersValidator()
//     {
//         RuleFor(x => x.Aantal)
//             .Cascade(CascadeMode.Stop)
//             .NotNull()
//             .WithProblemCode(ProblemCode.RoadSegment.LaneCount.IsRequired)
//             .Must(x => RoadSegmentLaneCount.CanParseUsingDutchName(x) && RoadSegmentLaneCount.ParseUsingDutchName(x).IsValidForEdit())
//             .WithProblemCode(ProblemCode.RoadSegment.LaneCount.NotValid);
//
//         RuleFor(x => x.Richting)
//             .Cascade(CascadeMode.Stop)
//             .NotNull()
//             .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.IsRequired)
//             .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
//             .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.NotValid);
//     }
// }

public class CreateOutlinedRoadSegmentV2ParametersExamples : IExamplesProvider<CreateOutlinedRoadSegmentV2Parameters>
{
    public CreateOutlinedRoadSegmentV2Parameters GetExamples()
    {
        //TODO-pr implement example
        throw new NotImplementedException();
//         return new CreateOutlinedRoadSegmentV2Parameters
//         {
//             MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
// <gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
// </gml:LineString>",
//             Wegsegmentstatus = RoadSegmentStatus.InUse.Translation.Name,
//             MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.Translation.Name,
//             Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.Translation.Name,
//             Wegbeheerder = "44021",
//             Wegverharding = RoadSegmentSurfaceType.SolidSurface.Translation.Name,
//             Wegbreedte = "5",
//             AantalRijstroken = new RoadSegmentLaneParameters
//             {
//                 Aantal = "2",
//                 Richting = RoadSegmentLaneDirection.Forward.Translation.Name
//             }
//         };
    }
}
