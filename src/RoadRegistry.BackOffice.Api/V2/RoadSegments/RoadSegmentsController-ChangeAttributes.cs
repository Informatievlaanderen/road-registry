namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadSegmentsController
{
    private const string ChangeAttributesRoute = "acties/wijzigen/attributen";

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
    [SwaggerOperation(OperationId = nameof(ChangeRoadSegmentAttributesV2), Description = "Wijzig één of meerdere attribuutwaarden voor één of meerdere wegsegmenten.")]
    public async Task<IActionResult> ChangeRoadSegmentAttributesV2(
        [FromServices] IDocumentStore store,
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

            await using var session = store.LightweightSession();
            var existingRoadSegments = await LoadExistingRoadSegments(session, parameters, cancellationToken);

            var groups = TranslateAndValidate(parameters, existingRoadSegments);

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

    // VAL-35: attribute values may only be changed on a road segment with one of these statuses.
    private static readonly string[] ChangeAttributesAllowedStatuses =
    [
        RoadSegmentStatusV2.Gepland.ToString(),
        RoadSegmentStatusV2.Gerealiseerd.ToString(),
        RoadSegmentStatusV2.BuitenGebruik.ToString()
    ];

    private static async Task<IReadOnlyDictionary<int, string>> LoadExistingRoadSegments(IDocumentSession session, ChangeRoadSegmentAttributesV2Parameters parameters, CancellationToken cancellationToken)
    {
        var ids = parameters
            .Where(x => x.Wegsegmenten is not null)
            .SelectMany(x => x.Wegsegmenten!)
            .Distinct()
            .ToArray();

        var roadSegments = await session.LoadManyAsync<RoadSegmentReadItem>(cancellationToken, ids);

        return roadSegments
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => x.Id, x => x.Status);
    }

    private static IReadOnlyList<ChangeRoadSegmentAttributesV2Group> TranslateAndValidate(ChangeRoadSegmentAttributesV2Parameters parameters, IReadOnlyDictionary<int, string> existingRoadSegments)
    {
        var failures = new List<ValidationFailure>();

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

            // VAL-9: every id must be an existing, non-removed road segment.
            var unknownIds = item.Wegsegmenten.Where(id => !existingRoadSegments.ContainsKey(id)).Distinct().ToList();
            if (unknownIds.Count > 0)
            {
                failures.Add(new ValidationFailure($"{path}.wegsegmenten", $"De wegsegmenten {string.Join(", ", unknownIds)} bestaan niet of zijn verwijderd."));
            }

            // VAL-35: every road segment must have status 'gepland', 'gerealiseerd' or 'buiten gebruik'.
            var invalidStatusIds = item.Wegsegmenten
                .Where(id => existingRoadSegments.TryGetValue(id, out var status) && !ChangeAttributesAllowedStatuses.Contains(status))
                .Distinct()
                .ToList();
            if (invalidStatusIds.Count > 0)
            {
                failures.Add(new ValidationFailure($"{path}.wegsegmenten", $"De wegsegmenten {string.Join(", ", invalidStatusIds)} hebben een status die verschilt van 'gepland', 'gerealiseerd' of 'buiten gebruik'."));
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
            if (!OrganizationId.AcceptsValue(element.Wegbeheerder))
            {
                failures.Add(new ValidationFailure(elementPath, $"De wegbeheerdercode {element.Wegbeheerder} is niet gekend in het Wegenregister."));
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

        failures.Add(new ValidationFailure(path, $"De straatnaamidentificator {identificator} komt niet overeen met een gekende, niet-verwijderde straatnaam in het Adressenregister."));
        return null;
    }
}

[DataContract(Name = "WegsegmentenV2AttribuutwaardenWijzigen")]
public class ChangeRoadSegmentAttributesV2Parameters : List<ChangeRoadSegmentAttributeV2Parameters>
{
}

public record ChangeRoadSegmentAttributeV2Parameters
{
    [DataMember(Name = "Wegsegmenten", Order = 0)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[]? Wegsegmenten { get; set; }

    [DataMember(Name = "Morfologie", Order = 1)]
    [JsonProperty("morfologie")]
    public MorfologieParameters[]? Morfologie { get; set; }

    [DataMember(Name = "Wegverharding", Order = 2)]
    [JsonProperty("wegverharding")]
    public WegverhardingParameters[]? Wegverharding { get; set; }

    [DataMember(Name = "Toegang", Order = 3)]
    [JsonProperty("toegang")]
    public ToegangParameters[]? Toegang { get; set; }

    [DataMember(Name = "Straatnaam", Order = 4)]
    [JsonProperty("straatnaam")]
    public StraatnaamParameters[]? Straatnaam { get; set; }

    [DataMember(Name = "Wegbeheerder", Order = 5)]
    [JsonProperty("wegbeheerder")]
    public WegbeheerderParameters[]? Wegbeheerder { get; set; }

    [DataMember(Name = "Wegcategorie", Order = 6)]
    [JsonProperty("wegcategorie")]
    public WegcategorieParameters[]? Wegcategorie { get; set; }

    [DataMember(Name = "VerkeerstypeAuto", Order = 7)]
    [JsonProperty("verkeerstypeAuto")]
    public VerkeerstypeParameters[]? VerkeerstypeAuto { get; set; }

    [DataMember(Name = "VerkeerstypeFiets", Order = 8)]
    [JsonProperty("verkeerstypeFiets")]
    public VerkeerstypeParameters[]? VerkeerstypeFiets { get; set; }

    [DataMember(Name = "VerkeerstypeVoetganger", Order = 9)]
    [JsonProperty("verkeerstypeVoetganger")]
    public VerkeerstypeVoetgangerParameters[]? VerkeerstypeVoetganger { get; set; }
}

public record VanTotParameters
{
    [DataMember(Name = "vanPositie", Order = 1)]
    [JsonProperty("vanPositie")]
    public double? VanPositie { get; set; }

    [DataMember(Name = "totPositie", Order = 2)]
    [JsonProperty("totPositie")]
    public double? TotPositie { get; set; }
}

public record MorfologieParameters : VanTotParameters
{
    [DataMember(Name = "morfologie", Order = 3)]
    [JsonProperty("morfologie", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphologyV2))]
    public string Morfologie { get; set; }
}

public record WegverhardingParameters : VanTotParameters
{
    [DataMember(Name = "wegverharding", Order = 3)]
    [JsonProperty("wegverharding", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceTypeV2))]
    public string Wegverharding { get; set; }
}

public record ToegangParameters : VanTotParameters
{
    [DataMember(Name = "toegang", Order = 3)]
    [JsonProperty("toegang", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestrictionV2))]
    public string Toegang { get; set; }
}

public record WegcategorieParameters : VanTotParameters
{
    [DataMember(Name = "wegcategorie", Order = 3)]
    [JsonProperty("wegcategorie", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentCategoryV2))]
    public string Wegcategorie { get; set; }
}

public record StraatnaamParameters : VanTotParameters
{
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    [DataMember(Name = "identificator", Order = 3)]
    [JsonProperty("identificator", Required = Required.Always)]
    public string Identificator { get; set; }
}

public record WegbeheerderParameters : VanTotParameters
{
    [DataMember(Name = "kant", Order = 0)]
    [JsonProperty("kant")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAttributeSide))]
    public string Kant { get; set; }

    [DataMember(Name = "wegbeheerder", Order = 3)]
    [JsonProperty("wegbeheerder", Required = Required.Always)]
    public string Wegbeheerder { get; set; }
}

public record VerkeerstypeParameters : VanTotParameters
{
    [DataMember(Name = "richting", Order = 3)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentTrafficDirection))]
    public string Richting { get; set; }
}

public record VerkeerstypeVoetgangerParameters : VanTotParameters
{
    [DataMember(Name = "richting", Order = 3)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentPedestrianTrafficDirection))]
    public string Richting { get; set; }
}
