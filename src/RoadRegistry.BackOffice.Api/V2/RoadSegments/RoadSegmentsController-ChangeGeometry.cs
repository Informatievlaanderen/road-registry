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
using RoadRegistry.BackOffice.Abstractions.Extensions;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;

public partial class RoadSegmentsController
{
    private const string ChangeGeometryRoute = "{id}/acties/wijzigen/geometrie";

    /// <summary>
    ///     Wijzig de geometrie van een wegsegment.
    /// </summary>
    /// <param name="idValidator"></param>
    /// <param name="parameters"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeGeometryRoute, Name = nameof(ChangeRoadSegmentGeometryV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeRoadSegmentGeometryV2Parameters), typeof(ChangeRoadSegmentGeometryV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeRoadSegmentGeometryV2), Description = "Wijzig de geometrie van een wegsegment. Wegknopen op het start- of eindpunt verplaatsen mee, net als de aansluitende wegsegmenten.")]
    public async Task<IActionResult> ChangeRoadSegmentGeometryV2(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromBody] ChangeRoadSegmentGeometryV2Parameters parameters,
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            // Only the shape of the request is validated here. Everything content related (does the road segment
            // exist, does it have an editable status, which road nodes and connected segments move along) is validated
            // by the domain, which is the only place that knows the surrounding network.
            var (geometry, attributes) = TranslateAndValidateGeometryChange(parameters);

            var sqsRequest = new ChangeRoadSegmentGeometryV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                RoadSegmentId = new RoadSegmentId(id),
                Geometry = geometry,
                // VAL-5: only a holder of the 'ingemeten' scope may touch measured geometries. Which segments are
                // actually measured is decided by the domain - a road node dragging a measured segment along counts
                // too - so the entitlement travels with the request rather than the verdict.
                MayModifyMeasuredRoadSegments = HasIngemetenWegScope(),
                Morphology = attributes.Morphology,
                SurfaceType = attributes.SurfaceType,
                AccessRestriction = attributes.AccessRestriction,
                Category = attributes.Category,
                StreetName = attributes.StreetName,
                MaintenanceAuthority = attributes.MaintenanceAuthority,
                CarTrafficDirection = attributes.CarTrafficDirection,
                BikeTrafficDirection = attributes.BikeTrafficDirection,
                PedestrianTrafficDirection = attributes.PedestrianTrafficDirection
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }

    private bool HasIngemetenWegScope()
    {
        // A scope claim can arrive either as one claim per scope or as a single space separated claim value,
        // depending on the authentication scheme, so both shapes are accounted for.
        return User.FindAll(AcmIdmClaimTypes.Scope)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Any(scope => string.Equals(scope, Scopes.DvWrIngemetenWegBeheer, StringComparison.OrdinalIgnoreCase));
    }

    private static (RoadSegmentGeometry Geometry, ChangeGeometryAttributes Attributes) TranslateAndValidateGeometryChange(
        ChangeRoadSegmentGeometryV2Parameters parameters)
    {
        var failures = new List<ValidationFailure>();

        if (parameters is null)
        {
            throw new ValidationException([new ValidationFailure(nameof(parameters), "Ongeldige JSON.")]);
        }

        var geometry = ParseGeometry(parameters.WegsegmentGeometrie, failures);

        // VAL-46 t.e.m. VAL-54: unlike the attribute change, where any subset may be given, a geometry change has to
        // carry every mandatory attribute. The positions the values apply to are relative to the geometry, so a
        // segment whose geometry moved has no meaningful attribute positions left to fall back on.
        var attributes = new ChangeGeometryAttributes
        {
            Morphology = ParseRequiredPositionValues(parameters.Morfologie, "morfologie", x => x.Morfologie, RoadSegmentMorphologyV2.CanParseUsingDutchName, RoadSegmentMorphologyV2.ParseUsingDutchName, failures),
            SurfaceType = ParseRequiredPositionValues(parameters.Wegverharding, "wegverharding", x => x.Wegverharding, RoadSegmentSurfaceTypeV2.CanParseUsingDutchName, RoadSegmentSurfaceTypeV2.ParseUsingDutchName, failures),
            AccessRestriction = ParseRequiredPositionValues(parameters.Toegang, "toegang", x => x.Toegang, RoadSegmentAccessRestrictionV2.CanParseUsingDutchName, RoadSegmentAccessRestrictionV2.ParseUsingDutchName, failures),
            Category = ParseRequiredPositionValues(parameters.Wegcategorie, "wegcategorie", x => x.Wegcategorie, RoadSegmentCategoryV2.CanParseUsingDutchName, RoadSegmentCategoryV2.ParseUsingDutchName, failures),
            CarTrafficDirection = ParseRequiredDirectionValues(parameters.VerkeerstypeAuto, "verkeerstypeAuto", RoadSegmentTrafficDirection.CanParseUsingDutchName, RoadSegmentTrafficDirection.ParseUsingDutchName, failures),
            BikeTrafficDirection = ParseRequiredDirectionValues(parameters.VerkeerstypeFiets, "verkeerstypeFiets", RoadSegmentTrafficDirection.CanParseUsingDutchName, RoadSegmentTrafficDirection.ParseUsingDutchName, failures),
            PedestrianTrafficDirection = ParseRequiredDirectionValues(parameters.VerkeerstypeVoetganger, "verkeerstypeVoetganger", RoadSegmentPedestrianTrafficDirection.CanParseUsingDutchName, RoadSegmentPedestrianTrafficDirection.ParseUsingDutchName, failures),
            StreetName = ParseRequired(parameters.Straatnaam, "straatnaam", failures, items => ParseStreetName(items, "straatnaam", failures)),
            MaintenanceAuthority = ParseRequired(parameters.Wegbeheerder, "wegbeheerder", failures, items => ParseMaintenanceAuthority(items, "wegbeheerder", failures))
        };

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return (geometry!, attributes);
    }

    // VAL-13 t.e.m. VAL-17. The remaining geometry validations (VAL-18 t.e.m. VAL-22) need the surrounding road
    // network and are left to the domain.
    private static RoadSegmentGeometry? ParseGeometry(string? gml, List<ValidationFailure> failures)
    {
        const string path = "wegsegmentGeometrie";

        if (string.IsNullOrEmpty(gml))
        {
            failures.Add(new ValidationFailure(path, "De parameter 'wegsegmentGeometrie' is verplicht."));
            return null;
        }
        if (!GeometryTranslator.GmlIsValidLineString(gml))
        {
            failures.Add(new ValidationFailure(path, "De opgegeven geometrie is geen geldige LineString in gml 3.2."));
            return null;
        }

        var geometry = GeometryTranslator.ParseGmlLineString(gml);
        if (geometry.SRID != WellknownSrids.Lambert08)
        {
            failures.Add(new ValidationFailure(path, "De opgegeven geometrie heeft niet het gewenste coördinatenstelsel: Lambert 2008 (EPSG:3812)."));
            return null;
        }
        if (geometry.Length < Distances.RoadSegmentV2MinimumLength)
        {
            failures.Add(new ValidationFailure(path, $"De opgegeven geometrie is korter dan {Distances.RoadSegmentV2MinimumLength.ToInvariantString()} meter."));
            return null;
        }
        if (geometry.GetSingleLineString().ContainsVertexTooCloseToAnother(Distances.MinimumDistanceBetweenVertices))
        {
            failures.Add(new ValidationFailure(path, "De afstand tussen twee opeenvolgende vertices van de opgegeven geometrie bedraagt niet overal 15cm."));
            return null;
        }

        return geometry.ToRoadSegmentGeometry();
    }

    private static IReadOnlyList<AttributeValue<T>>? ParseRequiredPositionValues<TParameters, T>(
        TParameters[]? items,
        string parameterName,
        Func<TParameters, string?> valueSelector,
        Func<string, bool> canParse,
        Func<string, T> parse,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
        where T : notnull
    {
        return ParseRequired(items, parameterName, failures,
            x => ParsePositionValues(x, parameterName, parameterName, valueSelector, canParse, parse, failures));
    }

    private static IReadOnlyList<AttributeValue<T>>? ParseRequiredDirectionValues<TParameters, T>(
        TParameters[]? items,
        string parameterName,
        Func<string, bool> canParse,
        Func<string, T> parse,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
        where T : notnull
    {
        return ParseRequired(items, parameterName, failures,
            x => ParseDirectionValues(x, parameterName, item => GetRichting(item), canParse, parse, failures));
    }

    private static string? GetRichting(VanTotParameters parameters)
    {
        return parameters switch
        {
            VerkeerstypeParameters x => x.Richting,
            VerkeerstypeVoetgangerParameters x => x.Richting,
            _ => null
        };
    }

    private static TResult? ParseRequired<TParameters, TResult>(
        TParameters[]? items,
        string parameterName,
        List<ValidationFailure> failures,
        Func<TParameters[], TResult?> parse)
        where TResult : class
    {
        if (items is null || items.Length == 0)
        {
            failures.Add(new ValidationFailure(parameterName, $"De parameter '{parameterName}' is verplicht."));
            return null;
        }

        return parse(items);
    }

    private sealed record ChangeGeometryAttributes
    {
        public IReadOnlyList<AttributeValue<RoadSegmentMorphologyV2>>? Morphology { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentSurfaceTypeV2>>? SurfaceType { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentAccessRestrictionV2>>? AccessRestriction { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentCategoryV2>>? Category { get; init; }
        public IReadOnlyList<SidedAttributeValue<StreetNameLocalId>>? StreetName { get; init; }
        public IReadOnlyList<SidedAttributeValue<OrganizationId>>? MaintenanceAuthority { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentTrafficDirection>>? CarTrafficDirection { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentTrafficDirection>>? BikeTrafficDirection { get; init; }
        public IReadOnlyList<AttributeValue<RoadSegmentPedestrianTrafficDirection>>? PedestrianTrafficDirection { get; init; }
    }
}

[DataContract(Name = "WegsegmentV2GeometrieWijzigen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2GeometrieWijzigen")]
public record ChangeRoadSegmentGeometryV2Parameters
{
    /// <summary>
    ///     GML-lijngeometrie van het wegsegment, in het coördinatenstelsel Lambert 2008 (EPSG:3812).
    /// </summary>
    [DataMember(Name = "WegsegmentGeometrie", Order = 1)]
    [JsonProperty("wegsegmentGeometrie", Required = Required.Always)]
    public string WegsegmentGeometrie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de vorm beschrijft die een weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "Morfologie", Order = 2)]
    [JsonProperty("morfologie", Required = Required.Always)]
    public MorfologieParameters[]? Morfologie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft welk type verharding van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 3)]
    [JsonProperty("wegverharding", Required = Required.Always)]
    public WegverhardingParameters[]? Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke mate een weg toegankelijk is voor weggebruikers in het algemeen.
    /// </summary>
    [DataMember(Name = "Toegang", Order = 4)]
    [JsonProperty("toegang", Required = Required.Always)]
    public ToegangParameters[]? Toegang { get; set; }

    /// <summary>
    ///     De straatnaam uit het Adressenregister gekoppeld aan het wegsegment.
    /// </summary>
    [DataMember(Name = "Straatnaam", Order = 5)]
    [JsonProperty("straatnaam", Required = Required.Always)]
    public StraatnaamParameters[]? Straatnaam { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft wie verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 6)]
    [JsonProperty("wegbeheerder", Required = Required.Always)]
    public WegbeheerderParameters[]? Wegbeheerder { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de categorie weergeeft van een weg zoals vastgelegd door de Vlaamse Overheid.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 7)]
    [JsonProperty("wegcategorie", Required = Required.Always)]
    public WegcategorieParameters[]? Wegcategorie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor auto's.
    /// </summary>
    [DataMember(Name = "VerkeerstypeAuto", Order = 8)]
    [JsonProperty("verkeerstypeAuto", Required = Required.Always)]
    public VerkeerstypeParameters[]? VerkeerstypeAuto { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor fietsers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeFiets", Order = 9)]
    [JsonProperty("verkeerstypeFiets", Required = Required.Always)]
    public VerkeerstypeParameters[]? VerkeerstypeFiets { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft of het wegsegment toegankelijk is voor voetgangers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeVoetganger", Order = 10)]
    [JsonProperty("verkeerstypeVoetganger", Required = Required.Always)]
    public VerkeerstypeVoetgangerParameters[]? VerkeerstypeVoetganger { get; set; }
}

public class ChangeRoadSegmentGeometryV2ParametersExamples : IExamplesProvider<ChangeRoadSegmentGeometryV2Parameters>
{
    public ChangeRoadSegmentGeometryV2Parameters GetExamples()
    {
        var geometry = GeometryExtensions.WithSrid(new LineString([
            new(243234.8929999992, 160239.3830000013),
            new(243245.9949999973, 160238.7989999987),
            new(243261.3599999994, 160239.0),
            new(243279.0160000026, 160244.1570000015)
        ]), WellknownSrids.Lambert72);
        var length = geometry.Length.RoundToCm();

        return new ChangeRoadSegmentGeometryV2Parameters
        {
            WegsegmentGeometrie = geometry.EnsureLambert08().ConvertToGml(useHttpsSchema: false),
            // Alle verplichte attributen worden meegegeven, ook als ze dezelfde waarde hebben over de volledige
            // lengte: de nieuwe geometrie bepaalt de posities waarop ze van toepassing zijn.
            Morfologie =
            [
                new MorfologieParameters { VanPositie = 0, TotPositie = length, Morfologie = RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan.ToDutchString() }
            ],
            Wegverharding =
            [
                new WegverhardingParameters { VanPositie = 0, TotPositie = length, Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }
            ],
            Toegang =
            [
                new ToegangParameters { VanPositie = 0, TotPositie = length, Toegang = RoadSegmentAccessRestrictionV2.OpenbareWeg.ToDutchString() }
            ],
            Straatnaam =
            [
                new StraatnaamParameters
                {
                    Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = length,
                    Identificator = OsloNamespaces.StraatNaam.ToPuri(79632.ToString())
                }
            ],
            Wegbeheerder =
            [
                new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString(), VanPositie = 0, TotPositie = length, Wegbeheerder = "AWV114" }
            ],
            Wegcategorie =
            [
                new WegcategorieParameters { VanPositie = 0, TotPositie = length, Wegcategorie = RoadSegmentCategoryV2.LokaleOntsluitingsweg.ToDutchString() }
            ],
            VerkeerstypeAuto =
            [
                new VerkeerstypeParameters { VanPositie = 0, TotPositie = length, Richting = RoadSegmentTrafficDirection.Forward.ToDutchString() }
            ],
            VerkeerstypeFiets =
            [
                new VerkeerstypeParameters { VanPositie = 0, TotPositie = length, Richting = RoadSegmentTrafficDirection.Both.ToDutchString() }
            ],
            VerkeerstypeVoetganger =
            [
                new VerkeerstypeVoetgangerParameters { VanPositie = 0, TotPositie = length, Richting = RoadSegmentPedestrianTrafficDirection.Both.ToDutchString() }
            ]
        };
    }
}
