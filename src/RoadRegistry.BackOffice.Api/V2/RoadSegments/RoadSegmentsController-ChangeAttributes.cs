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
using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
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
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string ChangeAttributesRoute = "acties/wijzigen/attributen";

    // Everything the request names is loaded and changed as one unit of work, so the size of a single request is
    // bounded here rather than let downstream time out on it.
    private const int ChangeAttributesMaximumRoadSegmentCount = 1000;

    /// <summary>
    ///     Wijzig attribuutwaarde(n) voor één of meerdere wegsegmenten.
    /// </summary>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeAttributesRoute, Name = nameof(ChangeRoadSegmentAttributesV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeRoadSegmentAttributesV2Parameters), typeof(ChangeRoadSegmentAttributesV2ParametersExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeRoadSegmentAttributesV2), Description = "Wijzig één of meerdere attribuutwaarden voor één of meerdere wegsegmenten.")]
    public async Task<IActionResult> ChangeRoadSegmentAttributesV2(
        [FromBody] ChangeRoadSegmentAttributesV2Parameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-7: the body must be a non-empty array of change objects.
            if (parameters is null || parameters.Count == 0)
            {
                throw new ValidationException([new ValidationFailure(nameof(parameters), "Ongeldige JSON.")]);
            }

            // Only the shape of the request is validated here. Everything content related (does the road segment
            // exist, does it have an editable status, is it a V2 segment) is validated by the domain.
            var groups = TranslateAndValidate(parameters);

            var sqsRequest = new ChangeRoadSegmentAttributesV2SqsRequest
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

    private static IReadOnlyList<ChangeRoadSegmentAttributesV2Group> TranslateAndValidate(ChangeRoadSegmentAttributesV2Parameters parameters)
    {
        var failures = new List<ValidationFailure>();

        // A request that is too big is rejected on its own: reporting it together with the per-item failures of a
        // thousands-of-segments request would bury it.
        var uniqueRoadSegmentIdCount = parameters
            .Where(x => x.Wegsegmenten is not null)
            .SelectMany(x => x.Wegsegmenten!)
            .Distinct()
            .Count();
        if (uniqueRoadSegmentIdCount > ChangeAttributesMaximumRoadSegmentCount)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(parameters), $"Er kunnen maximaal {ChangeAttributesMaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er {uniqueRoadSegmentIdCount} opgegeven.")
            ]);
        }

        // VAL-32: the same attribute may only be specified once per road segment (across the whole request).
        var seenAttributePerSegment = new HashSet<(string Attribute, int RoadSegmentId)>();

        var groups = new List<ChangeRoadSegmentAttributesV2Group>();

        for (var i = 0; i < parameters.Count; i++)
        {
            var item = parameters[i];
            var path = $"[{i}]";

            // VAL-8: wegsegmenten is required.
            if (item.Wegsegmenten is null || item.Wegsegmenten.Length == 0)
            {
                failures.Add(new ValidationFailure($"{path}.wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
                continue;
            }

            // VAL-7: at least one attribute must be present.
            var hasAnyAttribute =
                item.Morfologie is not null || item.Wegverharding is not null || item.Toegang is not null ||
                item.Straatnaam is not null || item.Wegbeheerder is not null || item.Wegcategorie is not null ||
                item.VerkeerstypeAuto is not null || item.VerkeerstypeFiets is not null || item.VerkeerstypeVoetganger is not null;
            if (!hasAnyAttribute)
            {
                failures.Add(new ValidationFailure(path, "Minstens één attribuut is verplicht."));
                continue;
            }

            // VAL-32
            foreach (var (attribute, present) in new[]
                     {
                         ("morfologie", item.Morfologie is not null),
                         ("wegverharding", item.Wegverharding is not null),
                         ("toegang", item.Toegang is not null),
                         ("straatnaam", item.Straatnaam is not null),
                         ("wegbeheerder", item.Wegbeheerder is not null),
                         ("wegcategorie", item.Wegcategorie is not null),
                         ("verkeerstypeAuto", item.VerkeerstypeAuto is not null),
                         ("verkeerstypeFiets", item.VerkeerstypeFiets is not null),
                         ("verkeerstypeVoetganger", item.VerkeerstypeVoetganger is not null)
                     })
            {
                if (!present)
                {
                    continue;
                }
                foreach (var id in item.Wegsegmenten)
                {
                    if (!seenAttributePerSegment.Add((attribute, id)))
                    {
                        failures.Add(new ValidationFailure(path, $"De parameter '{attribute}' werd meermaals meegegeven voor wegsegment {id}."));
                    }
                }
            }

            // The positions of each attribute must cover the (side of the) segment as one gapless run starting at 0.
            // The length of the road segments is unknown here - they are deliberately not fetched - so where the last
            // record ends is left to the domain.
            ValidateAttributePositions(item.Morfologie, $"{path}.morfologie", null, ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.Wegverharding, $"{path}.wegverharding", null, ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.Toegang, $"{path}.toegang", null, ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.Wegcategorie, $"{path}.wegcategorie", null, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.VerkeerstypeAuto, $"{path}.verkeerstypeAuto", null, ProblemCode.RoadSegment.CarTrafficDirection.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.VerkeerstypeFiets, $"{path}.verkeerstypeFiets", null, ProblemCode.RoadSegment.BikeTrafficDirection.DynamicAttributeProblemCodes, failures);
            ValidateAttributePositions(item.VerkeerstypeVoetganger, $"{path}.verkeerstypeVoetganger", null, ProblemCode.RoadSegment.PedestrianTrafficDirection.DynamicAttributeProblemCodes, failures);
            ValidateSidedAttributePositions(item.Straatnaam, x => x.Kant, $"{path}.straatnaam", null, ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes, failures);
            ValidateSidedAttributePositions(item.Wegbeheerder, x => x.Kant, $"{path}.wegbeheerder", null, ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes, failures);

            groups.Add(new ChangeRoadSegmentAttributesV2Group
            {
                RoadSegmentIds = item.Wegsegmenten.Select(x => new RoadSegmentId(x)).ToList(),
                Morphology = ParsePositionValues(item.Morfologie, $"{path}.morfologie", "morfologie", x => x.Morfologie, RoadSegmentMorphologyV2.CanParseUsingDutchName, RoadSegmentMorphologyV2.ParseUsingDutchName, failures),
                SurfaceType = ParsePositionValues(item.Wegverharding, $"{path}.wegverharding", "wegverharding", x => x.Wegverharding, RoadSegmentSurfaceTypeV2.CanParseUsingDutchName, RoadSegmentSurfaceTypeV2.ParseUsingDutchName, failures),
                AccessRestriction = ParsePositionValues(item.Toegang, $"{path}.toegang", "toegang", x => x.Toegang, RoadSegmentAccessRestrictionV2.CanParseUsingDutchName, RoadSegmentAccessRestrictionV2.ParseUsingDutchName, failures),
                Category = ParsePositionValues(item.Wegcategorie, $"{path}.wegcategorie", "wegcategorie", x => x.Wegcategorie, RoadSegmentCategoryV2.CanParseUsingDutchName, RoadSegmentCategoryV2.ParseUsingDutchName, failures),
                CarTrafficDirection = ParseDirectionValues(item.VerkeerstypeAuto, $"{path}.verkeerstypeAuto", x => x.Richting, RoadSegmentTrafficDirection.CanParseUsingDutchName, RoadSegmentTrafficDirection.ParseUsingDutchName, failures),
                BikeTrafficDirection = ParseDirectionValues(item.VerkeerstypeFiets, $"{path}.verkeerstypeFiets", x => x.Richting, RoadSegmentTrafficDirection.CanParseUsingDutchName, RoadSegmentTrafficDirection.ParseUsingDutchName, failures),
                PedestrianTrafficDirection = ParseDirectionValues(item.VerkeerstypeVoetganger, $"{path}.verkeerstypeVoetganger", x => x.Richting, RoadSegmentPedestrianTrafficDirection.CanParseUsingDutchName, RoadSegmentPedestrianTrafficDirection.ParseUsingDutchName, failures),
                StreetName = ParseStreetName(item.Straatnaam, $"{path}.straatnaam", failures),
                MaintenanceAuthority = ParseMaintenanceAuthority(item.Wegbeheerder, $"{path}.wegbeheerder", failures)
            });
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return groups;
    }

    private static RoadSegmentPositionV2? ParsePosition(double? value, string path, string subParameter, List<ValidationFailure> failures)
    {
        if (value is null)
        {
            return null;
        }
        if (value < 0)
        {
            failures.Add(new ValidationFailure(path, $"De {subParameter} {value} heeft een ongeldige waarde."));
            return null;
        }
        return new RoadSegmentPositionV2(value.Value);
    }

    private static void ValidateAttributePositions<TParameters>(
        TParameters[]? items,
        string path,
        double? geometryLength,
        ProblemCode.RoadSegment.DynamicAttributeProblemCodes problemCodes,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
    {
        if (items is null)
        {
            return;
        }

        failures.AddRange(RoadSegmentAttributePositionsValidator.Validate(
            items.Select(x => (x.VanPositie, x.TotPositie)), path, geometryLength, problemCodes));
    }

    private static void ValidateSidedAttributePositions<TParameters>(
        TParameters[]? items,
        Func<TParameters, string?> kantSelector,
        string path,
        double? geometryLength,
        ProblemCode.RoadSegment.DynamicAttributeProblemCodes problemCodes,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
    {
        if (items is null)
        {
            return;
        }

        failures.AddRange(RoadSegmentAttributePositionsValidator.ValidateSided(
            items.Select(x => (kantSelector(x), x.VanPositie, x.TotPositie)), path, geometryLength, problemCodes));
    }

    private static IReadOnlyList<AttributeValue<T>>? ParsePositionValues<TParameters, T>(
        TParameters[]? items,
        string path,
        string valueSubParameter,
        Func<TParameters, string?> valueSelector,
        Func<string, bool> canParse,
        Func<string, T> parse,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
        where T : notnull
    {
        if (items is null)
        {
            return null;
        }

        var result = new List<AttributeValue<T>>();
        for (var i = 0; i < items.Length; i++)
        {
            var element = items[i];
            var elementPath = $"{path}[{i}]";
            var from = ParsePosition(element.VanPositie, $"{elementPath}.vanPositie", "vanPositie", failures);
            var to = ParsePosition(element.TotPositie, $"{elementPath}.totPositie", "totPositie", failures);

            var rawValue = valueSelector(element);
            if (string.IsNullOrEmpty(rawValue))
            {
                failures.Add(new ValidationFailure(elementPath, $"'{valueSubParameter}' is verplicht binnen elk object in de array '{path}'."));
                continue;
            }
            if (!canParse(rawValue))
            {
                failures.Add(new ValidationFailure(elementPath, $"De parameter '{valueSubParameter}' heeft een ongeldige waarde."));
                continue;
            }

            result.Add(new AttributeValue<T>(from, to, parse(rawValue)));
        }
        return result;
    }

    private static IReadOnlyList<AttributeValue<T>>? ParseDirectionValues<TParameters, T>(
        TParameters[]? items,
        string path,
        Func<TParameters, string?> richtingSelector,
        Func<string, bool> canParse,
        Func<string, T> parse,
        List<ValidationFailure> failures)
        where TParameters : VanTotParameters
        where T : notnull
    {
        if (items is null)
        {
            return null;
        }

        var result = new List<AttributeValue<T>>();
        for (var i = 0; i < items.Length; i++)
        {
            var element = items[i];
            var elementPath = $"{path}[{i}]";
            var from = ParsePosition(element.VanPositie, $"{elementPath}.vanPositie", "vanPositie", failures);
            var to = ParsePosition(element.TotPositie, $"{elementPath}.totPositie", "totPositie", failures);

            var richting = richtingSelector(element);
            if (string.IsNullOrEmpty(richting))
            {
                failures.Add(new ValidationFailure(elementPath, $"'richting' is verplicht binnen elk object in de array '{path}'."));
                continue;
            }
            if (!canParse(richting))
            {
                failures.Add(new ValidationFailure(elementPath, "De parameter 'richting' heeft een ongeldige waarde."));
                continue;
            }

            result.Add(new AttributeValue<T>(from, to, parse(richting)));
        }
        return result;
    }

    private static IReadOnlyList<SidedAttributeValue<StreetNameLocalId>>? ParseStreetName(StraatnaamParameters[]? items, string path, List<ValidationFailure> failures)
    {
        if (items is null)
        {
            return null;
        }

        var result = new List<SidedAttributeValue<StreetNameLocalId>>();
        for (var i = 0; i < items.Length; i++)
        {
            var element = items[i];
            var elementPath = $"{path}[{i}]";
            var from = ParsePosition(element.VanPositie, $"{elementPath}.vanPositie", "vanPositie", failures);
            var to = ParsePosition(element.TotPositie, $"{elementPath}.totPositie", "totPositie", failures);
            var side = ParseSide(element.Kant, elementPath, failures);

            if (string.IsNullOrEmpty(element.Identificator))
            {
                failures.Add(new ValidationFailure(elementPath, $"'identificator' is verplicht binnen elk object in de array '{path}'."));
                continue;
            }
            var streetNameId = ParseStreetNameId(element.Identificator, elementPath, failures);
            if (side is null || streetNameId is null)
            {
                continue;
            }

            result.Add(new SidedAttributeValue<StreetNameLocalId>(side, from, to, streetNameId.Value));
        }
        return result;
    }

    private static IReadOnlyList<SidedAttributeValue<OrganizationId>>? ParseMaintenanceAuthority(WegbeheerderParameters[]? items, string path, List<ValidationFailure> failures)
    {
        if (items is null)
        {
            return null;
        }

        var result = new List<SidedAttributeValue<OrganizationId>>();
        for (var i = 0; i < items.Length; i++)
        {
            var element = items[i];
            var elementPath = $"{path}[{i}]";
            var from = ParsePosition(element.VanPositie, $"{elementPath}.vanPositie", "vanPositie", failures);
            var to = ParsePosition(element.TotPositie, $"{elementPath}.totPositie", "totPositie", failures);
            var side = ParseSide(element.Kant, elementPath, failures);

            if (string.IsNullOrEmpty(element.Wegbeheerder))
            {
                failures.Add(new ValidationFailure(elementPath, $"'wegbeheerder' is verplicht binnen elk object in de array '{path}'."));
                continue;
            }
            // Only the shape is checked here: whether the organization is known is validated against the organization
            // cache further down the line.
            if (!OrganizationId.AcceptsValue(element.Wegbeheerder))
            {
                failures.Add(new ValidationFailure(elementPath, $"De wegbeheerdercode {element.Wegbeheerder} is ongeldig."));
                continue;
            }
            if (side is null)
            {
                continue;
            }

            result.Add(new SidedAttributeValue<OrganizationId>(side, from, to, new OrganizationId(element.Wegbeheerder)));
        }
        return result;
    }

    private static RoadSegmentAttributeSide? ParseSide(string? kant, string path, List<ValidationFailure> failures)
    {
        if (string.IsNullOrEmpty(kant))
        {
            failures.Add(new ValidationFailure(path, $"'kant' is verplicht binnen elk object in de array '{path}'."));
            return null;
        }
        if (!RoadSegmentAttributeSide.CanParseUsingDutchName(kant))
        {
            failures.Add(new ValidationFailure(path, "De parameter 'kant' heeft een ongeldige waarde."));
            return null;
        }
        return RoadSegmentAttributeSide.ParseUsingDutchName(kant);
    }

    private static StreetNameLocalId? ParseStreetNameId(string identificator, string path, List<ValidationFailure> failures)
    {
        if (string.Equals(identificator, "niet van toepassing", StringComparison.OrdinalIgnoreCase))
        {
            return StreetNameLocalId.NotApplicable;
        }

        var lastSegment = identificator.TrimEnd('/').Split('/').LastOrDefault();
        if (int.TryParse(lastSegment, out var id) && StreetNameLocalId.Accepts(id))
        {
            return new StreetNameLocalId(id);
        }

        // Only the shape is checked here: whether the street name exists and has the right status is validated against
        // the street name registry further down the line.
        failures.Add(new ValidationFailure(path, $"De straatnaamidentificator {identificator} is ongeldig."));
        return null;
    }
}

[DataContract(Name = "WegsegmentenV2AttribuutwaardenWijzigen")]
public class ChangeRoadSegmentAttributesV2Parameters : List<ChangeRoadSegmentAttributeV2Parameters>
{
}

/// <summary>
///     Een groep wegsegmenten samen met de attribuutwaarden die erop gewijzigd worden. Enkel de opgegeven attributen
///     worden gewijzigd; de overige blijven ongewijzigd.
/// </summary>
public record ChangeRoadSegmentAttributeV2Parameters
{
    /// <summary>
    ///     Objectidentificatoren van de wegsegmenten waarop de wijziging van toepassing is.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 0)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[]? Wegsegmenten { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de vorm beschrijft die een weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "Morfologie", Order = 1)]
    [JsonProperty("morfologie")]
    public MorfologieParameters[]? Morfologie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft welk type verharding van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 2)]
    [JsonProperty("wegverharding")]
    public WegverhardingParameters[]? Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke mate een weg toegankelijk is voor weggebruikers in het algemeen, ongeacht het type weggebruiker (voetgangers, fietsers, etc.).
    /// </summary>
    [DataMember(Name = "Toegang", Order = 3)]
    [JsonProperty("toegang")]
    public ToegangParameters[]? Toegang { get; set; }

    /// <summary>
    ///     De straatnaam uit het Adressenregister gekoppeld aan het wegsegment.
    /// </summary>
    [DataMember(Name = "Straatnaam", Order = 4)]
    [JsonProperty("straatnaam")]
    public StraatnaamParameters[]? Straatnaam { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft wie verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 5)]
    [JsonProperty("wegbeheerder")]
    public WegbeheerderParameters[]? Wegbeheerder { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de categorie weergeeft van een weg zoals vastgelegd door de Vlaamse Overheid.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 6)]
    [JsonProperty("wegcategorie")]
    public WegcategorieParameters[]? Wegcategorie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor auto’s.
    /// </summary>
    [DataMember(Name = "VerkeerstypeAuto", Order = 7)]
    [JsonProperty("verkeerstypeAuto")]
    public VerkeerstypeParameters[]? VerkeerstypeAuto { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor fietsers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeFiets", Order = 8)]
    [JsonProperty("verkeerstypeFiets")]
    public VerkeerstypeParameters[]? VerkeerstypeFiets { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft of het wegsegment toegankelijk is voor voetgangers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeVoetganger", Order = 9)]
    [JsonProperty("verkeerstypeVoetganger")]
    public VerkeerstypeVoetgangerParameters[]? VerkeerstypeVoetganger { get; set; }
}

public record VanTotParameters
{
    /// <summary>
    ///     Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "vanPositie", Order = 1)]
    [JsonProperty("vanPositie")]
    public double? VanPositie { get; set; }

    /// <summary>
    ///     Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "totPositie", Order = 2)]
    [JsonProperty("totPositie")]
    public double? TotPositie { get; set; }
}

/// <summary>
///     De morfologie die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record MorfologieParameters : VanTotParameters
{
    /// <summary>
    ///     De vorm die de weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "morfologie", Order = 3)]
    [JsonProperty("morfologie", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphologyV2))]
    public string Morfologie { get; set; }
}

/// <summary>
///     De wegverharding die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record WegverhardingParameters : VanTotParameters
{
    /// <summary>
    ///     Het type verharding dat van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "wegverharding", Order = 3)]
    [JsonProperty("wegverharding", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceTypeV2))]
    public string Wegverharding { get; set; }
}

/// <summary>
///     De toegankelijkheid die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record ToegangParameters : VanTotParameters
{
    /// <summary>
    ///     De mate waarin de weg toegankelijk is voor weggebruikers in het algemeen, ongeacht het type weggebruiker.
    /// </summary>
    [DataMember(Name = "toegang", Order = 3)]
    [JsonProperty("toegang", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestrictionV2))]
    public string Toegang { get; set; }
}

/// <summary>
///     De wegcategorie die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record WegcategorieParameters : VanTotParameters
{
    /// <summary>
    ///     De categorie van de weg zoals vastgelegd door de Vlaamse Overheid.
    /// </summary>
    [DataMember(Name = "wegcategorie", Order = 3)]
    [JsonProperty("wegcategorie", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentCategoryV2))]
    public string Wegcategorie { get; set; }
}

/// <summary>
///     De straatnaam die geldt voor een bepaald deel en een bepaalde kant van het wegsegment.
/// </summary>
public record StraatnaamParameters : VanTotParameters
{
    /// <summary>
    ///     Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    /// <summary>
    ///     Identificator van de straatnaam uit het Adressenregister.
    /// </summary>
    [DataMember(Name = "identificator", Order = 3)]
    [JsonProperty("identificator", Required = Required.Always)]
    public string Identificator { get; set; }
}

/// <summary>
///     De wegbeheerder die geldt voor een bepaald deel en een bepaalde kant van het wegsegment.
/// </summary>
public record WegbeheerderParameters : VanTotParameters
{
    /// <summary>
    ///     Kant waarop het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    /// <summary>
    ///     Organisatiecode van de wegbeheerder.
    /// </summary>
    [DataMember(Name = "wegbeheerder", Order = 3)]
    [JsonProperty("wegbeheerder", Required = Required.Always)]
    public string Wegbeheerder { get; set; }
}

/// <summary>
///     De rijrichting die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record VerkeerstypeParameters : VanTotParameters
{
    /// <summary>
    ///     De richting waarin het wegsegment toegankelijk is, t.o.v. de richting van het wegsegment (begin- naar eindknoop).
    /// </summary>
    [DataMember(Name = "richting", Order = 3)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentTrafficDirection))]
    public string Richting { get; set; }
}

/// <summary>
///     De toegankelijkheid voor voetgangers die geldt voor een bepaald deel van het wegsegment.
/// </summary>
public record VerkeerstypeVoetgangerParameters : VanTotParameters
{
    /// <summary>
    ///     De richting waarin het wegsegment toegankelijk is, t.o.v. de richting van het wegsegment (begin- naar eindknoop).
    /// </summary>
    [DataMember(Name = "richting", Order = 3)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentPedestrianTrafficDirection))]
    public string Richting { get; set; }
}

public class ChangeRoadSegmentAttributesV2ParametersExamples : IExamplesProvider<ChangeRoadSegmentAttributesV2Parameters>
{
    public ChangeRoadSegmentAttributesV2Parameters GetExamples()
    {
        return
        [
            // Eén attribuutwaarde voor de volledige lengte van meerdere wegsegmenten: vanPositie en totPositie mogen
            // dan weggelaten worden.
            new ChangeRoadSegmentAttributeV2Parameters
            {
                Wegsegmenten = [481110, 481111],
                Morfologie =
                [
                    new MorfologieParameters { Morfologie = RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan.ToDutchString() }
                ],
                Wegcategorie =
                [
                    new WegcategorieParameters { Wegcategorie = RoadSegmentCategoryV2.LokaleOntsluitingsweg.ToDutchString() }
                ]
            },
            // Attribuutwaarden die variëren over de lengte of per kant van het wegsegment. De opgegeven waarden
            // vervangen het volledige attribuut, dus ze moeten samen het volledige wegsegment bedekken.
            new ChangeRoadSegmentAttributeV2Parameters
            {
                Wegsegmenten = [481112],
                Wegverharding =
                [
                    new WegverhardingParameters { TotPositie = 50.5, Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() },
                    new WegverhardingParameters { VanPositie = 50.5, Wegverharding = RoadSegmentSurfaceTypeV2.Onverhard.ToDutchString() }
                ],
                Straatnaam =
                [
                    new StraatnaamParameters
                    {
                        Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                        Identificator = OsloNamespaces.StraatNaam.ToPuri(79632.ToString())
                    }
                ],
                Wegbeheerder =
                [
                    new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Links.ToDutchString(), Wegbeheerder = "AWV114" },
                    new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Rechts.ToDutchString(), Wegbeheerder = "AWV116" }
                ],
                VerkeerstypeAuto =
                [
                    new VerkeerstypeParameters { Richting = RoadSegmentTrafficDirection.Forward.ToDutchString() }
                ]
            }
        ];
    }
}
