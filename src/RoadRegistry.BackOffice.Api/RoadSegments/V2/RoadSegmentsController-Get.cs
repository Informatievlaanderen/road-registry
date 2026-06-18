namespace RoadRegistry.BackOffice.Api.RoadSegments.V2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string GetRoadSegmentRoute = "{id}";

    /// <summary>
    ///     Vraag een wegsegment op.
    /// </summary>
    /// <param name="id">De identificator van het wegsegment.</param>
    /// <param name="responseOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="410">Als het wegsegment is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetRoadSegmentRoute, Name = nameof(GetRoadSegmentV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WegsegmentV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(WegsegmentV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(RoadSegmentGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetRoadSegmentV2), Description = "Vraag een wegsegment op.")]
    public async Task<IActionResult> GetRoadSegmentV2(
        [FromRoute] int id,
        [FromServices] IOptions<ResponseOptions> responseOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id, cancellationToken);
        if (roadSegment is null)
        {
            return NotFound();
        }

        if (roadSegment.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new WegsegmentV2Detail
        {
            Identificator = new Identificator(responseOptions.Value.WegsegmentNaamruimte, roadSegment.RoadSegmentId.ToString(), roadSegment.Origin.Timestamp.ToDateTimeOffset()),
            WegsegmentGeometrie = new WegsegmentGeometrie
            {
                Geometrie =
                [
                    new WegsegmentGeometrieProjectie
                    {
                        Gml = roadSegment.Geometry.Lambert08.Value.ConvertToGml()
                    },
                    new WegsegmentGeometrieProjectie
                    {
                        Gml = roadSegment.Geometry.Lambert72.Value.ConvertToGml()
                    }
                ]
            },
            Wegsegmentstatus = RoadSegmentStatusV2.Parse(roadSegment.Status).ToDutchString(),
            Beginknoop = roadSegment.StartNodeId is not null
                ? new WegknoopLink
                {
                    ObjectId = roadSegment.StartNodeId.ToString(),
                    Detail = string.Format(responseOptions.Value.WegknoopDetailUrlFormat, roadSegment.StartNodeId)
                }
                : null,
            Eindknoop = roadSegment.EndNodeId is not null
                ? new WegknoopLink
                {
                    ObjectId = roadSegment.EndNodeId.ToString(),
                    Detail = string.Format(responseOptions.Value.WegknoopDetailUrlFormat, roadSegment.EndNodeId)
                }
                : null
        };

        //TODO-pr to confirm what to return with V1 data
        if (roadSegment.IsV2)
        {
            result.GeometrieMethode = RoadSegmentGeometryDrawMethodV2.Parse(roadSegment.GeometryDrawMethod).ToString();
            result.Straatnaam = roadSegment.StreetNameId.Values
                .Select(x => new WegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = x.Side.ToWegsegmentKant(),
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Straatnaam = x.Value!.StreetNameId > 0 ? new StraatnaamLink
                    {
                        ObjectId = x.Value.StreetNameId.ToString(),
                        Detail = string.Format(responseOptions.Value.StraatnaamDetailUrlFormat, x.Value.StreetNameId),
                        GeografischeNaam = new StraatnaamGeografischeNaam
                        {
                            Taal = "nl",
                            Spelling = x.Value.DutchName
                        }
                    } : null
                })
                .ToArray();
            result.Morfologie = roadSegment.Morphology.Values
                .Select(x => new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Morfologie = RoadSegmentMorphologyV2.Parse(x.Value!).ToDutchString()
                })
                .ToArray();
            result.Toegang = roadSegment.AccessRestriction.Values
                .Select(x => new WegsegmentToegangAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Toegang = RoadSegmentAccessRestrictionV2.Parse(x.Value!).ToDutchString()
                })
                .ToArray();
            result.Wegbeheerder = roadSegment.MaintenanceAuthorityId.Values
                .Select(x => new WegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = x.Side.ToWegsegmentKant(),
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Wegbeheerder = new WegbeheerderObject
                    {
                        Code = x.Value.ToString(),
                        Label = "TODO-pr implement get wegbeheerder label"
                    }
                })
                .ToArray();
            result.Wegcategorie = roadSegment.Category.Values
                .Select(x => new WegsegmentWegcategorieAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Wegcategorie = RoadSegmentCategoryV2.Parse(x.Value!).ToDutchString()
                })
                .ToArray();
            result.Wegverharding = roadSegment.SurfaceType.Values
                .Select(x => new WegsegmentWegverhardingAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Wegverharding = RoadSegmentSurfaceTypeV2.Parse(x.Value!).ToDutchString()
                })
                .ToArray();
            result.VerkeerstypeAuto = roadSegment.CarTrafficDirection.Values
                .Select(x => new WegsegmentVerkeerstypeAutoAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Richting = x.Value!.ToDutchString()
                })
                .ToArray();
            result.VerkeerstypeFiets = roadSegment.BikeTrafficDirection.Values
                .Select(x => new WegsegmentVerkeerstypeFietsAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Richting = x.Value!.ToDutchString()
                })
                .ToArray();
            result.VerkeerstypeVoetganger = roadSegment.PedestrianTrafficDirection.Values
                .Select(x => new WegsegmentVerkeerstypeVoetgangerAttribuutWaarde
                {
                    VanPositie = x.From,
                    TotPositie = x.To,
                    Richting = x.Value!.ToDutchString()
                })
                .ToArray();
            result.EuropeseWegen = roadSegment.EuropeanRoadNumbers
                .Select(x => new EuropeseWegObject
                {
                    EuropeesWegnummer = x.ToString()
                })
                .ToArray();
            result.NationaleWegen = roadSegment.NationalRoadNumbers
                .Select(x => new NationaleWegObject
                {
                    NationaalWegnummer = x.ToString()
                })
                .ToArray();
            result.GelijkgrondseKruisingen = roadSegment.GradeJunctionIds
                .Select(x => new GelijkgrondseKruisingLink
                {
                    ObjectId = x.ToString(),
                    Detail = string.Format(responseOptions.Value.GelijkGrondseKruisingDetailUrlFormat, x)
                })
                .ToArray();
            result.OngelijkgrondseKruisingen = roadSegment.GradeSeparatedJunctionIds
                .Select(x => new OngelijkgrondseKruisingLink
                {
                    ObjectId = x.ToString(),
                    Detail = string.Format(responseOptions.Value.OngelijkGrondseKruisingDetailUrlFormat, x)
                })
                .ToArray();
        }

        return Ok(result);
    }
}

[DataContract(Name = "WegsegmentV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2Detail")]
public class WegsegmentV2Detail
{
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public Identificator Identificator { get; set; }

    /// <summary>
    ///     De middellijngeometrie van het wegsegment.
    /// </summary>
    [DataMember(Name = "wegsegmentGeometrie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegsegmentGeometrie WegsegmentGeometrie { get; set; }

    /// <summary>
    ///     De geometriemethode van het wegsegment.
    /// </summary>
    [DataMember(Name = "GeometrieMethode", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentGeometryDrawMethodV2))]
    public string GeometrieMethode { get; set; }

    /// <summary>
    ///     Beginknoop van het wegsegment.
    /// </summary>
    [DataMember(Name = "Beginknoop", Order = 4)]
    [JsonProperty]
    public WegknoopLink? Beginknoop { get; set; }

    /// <summary>
    ///     Eindknoop van het wegsegment.
    /// </summary>
    [DataMember(Name = "Eindknoop", Order = 5)]
    [JsonProperty]
    public WegknoopLink? Eindknoop { get; set; }

    /// <summary>
    ///     De straatnaam uit het Adressenregister gekoppeld aan het wegsegment.
    /// </summary>
    [DataMember(Name = "Straatnaam", Order = 6)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentStraatnaamAttribuutWaarde> Straatnaam { get; set; }

    /// <summary>
    ///     Attribuut dat de levensloopfase van een wegsegment aangeeft.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 7)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatusV2))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de vorm beschrijft die een weg aanneemt, rekening houdend met fysieke en verkeerskundige eigenschappen.
    /// </summary>
    [DataMember(Name = "Morfologie", Order = 8)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentMorfologieAttribuutWaarde> Morfologie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke mate een weg toegankelijk is voor weggebruikers in het algemeen, ongeacht het type weggebruiker (voetgangers, fietsers, etc.).
    /// </summary>
    [DataMember(Name = "Toegang", Order = 9)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentToegangAttribuutWaarde> Toegang { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft wie verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 10)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentWegbeheerderAttribuutWaarde> Wegbeheerder { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de categorie weergeeft van een weg zoals vastgelegd door de Vlaamse Overheid.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 11)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentWegcategorieAttribuutWaarde> Wegcategorie { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft welk type verharding van toepassing is op de weg.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 12)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentWegverhardingAttribuutWaarde> Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor auto’s.
    /// </summary>
    [DataMember(Name = "VerkeerstypeAuto", Order = 13)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentVerkeerstypeAutoAttribuutWaarde> VerkeerstypeAuto { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft in welke richting het wegsegment toegankelijk is voor fietsers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeFiets", Order = 14)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentVerkeerstypeFietsAttribuutWaarde> VerkeerstypeFiets { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat aangeeft of het wegsegment toegankelijk is voor voetgangers.
    /// </summary>
    [DataMember(Name = "VerkeerstypeVoetganger", Order = 15)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentVerkeerstypeVoetgangerAttribuutWaarde> VerkeerstypeVoetganger { get; set; }

    /// <summary>
    ///     Wegnummer(s) van Europese wegen waartoe het wegsegment behoort.
    /// </summary>
    [DataMember(Name = "EuropeseWegen", Order = 16)]
    [JsonProperty(Required = Required.DisallowNull)]
    public EuropeseWegObject[] EuropeseWegen { get; set; }

    /// <summary>
    ///     Wegnummer(s) van nationale wegen waartoe het wegsegment behoort.
    /// </summary>
    [DataMember(Name = "NationaleWegen", Order = 17)]
    [JsonProperty(Required = Required.DisallowNull)]
    public NationaleWegObject[] NationaleWegen { get; set; }

    /// <summary>
    ///     Een gelijkgrondse kruising is een relatie tussen twee wegsegmenten die elkaar gelijkgronds kruisen, zonder uitwisseling van verkeer tussen beide wegsegmenten.
    /// </summary>
    [DataMember(Name = "GelijkgrondseKruisingen", Order = 18)]
    [JsonProperty(Required = Required.DisallowNull)]
    public GelijkgrondseKruisingLink[] GelijkgrondseKruisingen { get; set; }

    /// <summary>
    ///     Een ongelijkgrondse kruising is een relatie tussen twee wegsegmenten die elkaar ongelijkgronds kruisen, en waar bij het ene wegsegment zich boven of onder het andere wegsegment bevindt.
    /// </summary>
    [DataMember(Name = "OngelijkgrondseKruisingen", Order = 19)]
    [JsonProperty(Required = Required.DisallowNull)]
    public OngelijkgrondseKruisingLink[] OngelijkgrondseKruisingen { get; set; }
}

[DataContract(Name = "WegknoopLink", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopLink")]
public class WegknoopLink
{
    /// <summary>
    /// De objectidentificator van de wegknoop.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public required string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de wegknoop.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public required string Detail { get; set; }
}

[DataContract(Name = "GelijkgrondseKruisingLink", Namespace = "")]
[CustomSwaggerSchemaId("GelijkgrondseKruisingLink")]
public class GelijkgrondseKruisingLink
{
    /// <summary>
    /// De objectidentificator van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public required string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public required string Detail { get; set; }
}

[DataContract(Name = "OngelijkgrondseKruisingLink", Namespace = "")]
[CustomSwaggerSchemaId("OngelijkgrondseKruisingLink")]
public class OngelijkgrondseKruisingLink
{
    /// <summary>
    /// De objectidentificator van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public required string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public required string Detail { get; set; }
}

[DataContract(Name = "StraatnaamLink", Namespace = "")]
[CustomSwaggerSchemaId("StraatnaamLink")]
public class StraatnaamLink
{
    /// <summary>
    /// De objectidentificator van de straatnaam.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public required string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de straatnaam.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public required string Detail { get; set; }

    /// <summary>
    /// De geografische naam van de straat in het Nederlands.
    /// </summary>
    [DataMember(Name = "GeografischeNaam", Order = 3)]
    [JsonProperty]
    public required StraatnaamGeografischeNaam GeografischeNaam { get; set; }
}

[DataContract(Name = "StraatnaamGeografischeNaam", Namespace = "")]
[CustomSwaggerSchemaId("StraatnaamGeografischeNaam")]
public class StraatnaamGeografischeNaam
{
    public required string Spelling { get; set; }
    public required string Taal { get; set; }
}

[DataContract(Name = "WegsegmentGeometrie", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentGeometrie")]
public class WegsegmentGeometrie
{
    /// <summary>
    /// Geometrie van het wegsegment.
    /// </summary>
    [DataMember(Name = "Geometrie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required IReadOnlyCollection<WegsegmentGeometrieProjectie> Geometrie { get; set; }

    /// <summary>
    /// GeoJSON-geometrietype.
    /// </summary>
    [DataMember(Name = "Type", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Type { get; } = "LineString";
}

[DataContract(Name = "WegsegmentGeometrieProjectie", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentGeometrieProjectie")]
public class WegsegmentGeometrieProjectie
{
    [DataMember(Name = "gml", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Gml { get; set; }
}

public enum WegsegmentKant
{
    Links,
    Rechts,
    Beide
}

internal static class WegsegmentKantExtensions
{
    public static WegsegmentKant ToWegsegmentKant(this RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => WegsegmentKant.Links,
            RoadSegmentAttributeSide.Right => WegsegmentKant.Rechts,
            RoadSegmentAttributeSide.Both => WegsegmentKant.Beide,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}

[DataContract(Name = "WegsegmentStraatnaamAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentStraatnaamAttribuutWaarde")]
public class WegsegmentStraatnaamAttribuutWaarde
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

    [DataMember(Name = "Straatnaam", Order = 4)]
    [JsonProperty(Required = Required.AllowNull)]
    public required StraatnaamLink? Straatnaam { get; set; }
}

[DataContract(Name = "WegsegmentMorfologieAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentMorfologieAttribuutWaarde")]
public class WegsegmentMorfologieAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Morfologie", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphologyV2))]
    public required string Morfologie { get; set; }
}

[DataContract(Name = "WegsegmentToegangAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentToegangAttribuutWaarde")]
public class WegsegmentToegangAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Toegang", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestrictionV2))]
    public required string Toegang { get; set; }
}

[DataContract(Name = "WegsegmentWegbeheerderAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentWegbeheerderAttribuutWaarde")]
public class WegsegmentWegbeheerderAttribuutWaarde
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
    /// Wegbeheerdersorganisatie
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegbeheerderObject Wegbeheerder { get; set; }
}
[DataContract(Name = "WegbeheerderObject", Namespace = "")]
[CustomSwaggerSchemaId("WegbeheerderObject")]
public class WegbeheerderObject
{
    /// <summary>
    /// Organisatiecode van de wegbeheerder.
    /// </summary>
    [DataMember(Name = "Code", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Code { get; set; }

    /// <summary>
    /// Organisatielabel van de wegbeheerder.
    /// </summary>
    [DataMember(Name = "Label", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Label { get; set; }
}

[DataContract(Name = "WegsegmentWegcategorieAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentWegcategorieAttribuutWaarde")]
public class WegsegmentWegcategorieAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Wegcategorie", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentCategoryV2))]
    public required string Wegcategorie { get; set; }
}

[DataContract(Name = "WegsegmentWegverhardingAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentWegverhardingAttribuutWaarde")]
public class WegsegmentWegverhardingAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Wegverharding", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceTypeV2))]
    public required string Wegverharding { get; set; }
}

[DataContract(Name = "WegsegmentVerkeerstypeAutoAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentVerkeerstypeAutoAttribuutWaarde")]
public class WegsegmentVerkeerstypeAutoAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Richting", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentTrafficDirection))]
    public required string Richting { get; set; }
}

[DataContract(Name = "WegsegmentVerkeerstypeFietsAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentVerkeerstypeFietsAttribuutWaarde")]
public class WegsegmentVerkeerstypeFietsAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Richting", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentTrafficDirection))]
    public required string Richting { get; set; }
}

[DataContract(Name = "WegsegmentVerkeerstypeVoetgangerAttribuutWaarde", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentVerkeerstypeVoetgangerAttribuutWaarde")]
public class WegsegmentVerkeerstypeVoetgangerAttribuutWaarde
{
    /// <summary>
    /// Positie vanaf waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double VanPositie { get; set; }

    /// <summary>
    /// Positie tot waar het attribuut van toepassing is.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required double TotPositie { get; set; }

    [DataMember(Name = "Richting", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentPedestrianTrafficDirection))]
    public required string Richting { get; set; }
}

[DataContract(Name = "WegsegmentV2EuropeseWeg", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2EuropeseWeg")]
public class EuropeseWegObject
{
    /// <summary>
    /// Nummer van de Europese weg.
    /// </summary>
    [DataMember(Name = "EuropeesWegnummer", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(EuropeanRoadNumber))]
    public required string EuropeesWegnummer { get; set; }
}

[DataContract(Name = "WegsegmentV2NationaleWeg", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2NationaleWeg")]
public class NationaleWegObject
{
    /// <summary>
    /// Nummer van de nationale weg.
    /// </summary>
    [DataMember(Name = "NationaalWegnummer", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string NationaalWegnummer { get; set; }
}

public class WegsegmentV2DetailResponseExamples : IExamplesProvider<WegsegmentV2Detail>
{
    public WegsegmentV2Detail GetExamples()
    {
        throw new NotImplementedException(); //TODO-pr implement example
        // return new GetRoadSegmentResponseV2
        // {
        //     Identificator = new WegsegmentIdentificatorV2("https://data.vlaanderen.be/id/wegsegment", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
        //     MiddellijnGeometrie = new GeoJSONMultiLineString
        //     {
        //         Coordinates = new[]
        //         {
        //             new[]
        //             {
        //                 new[] { 243234.8929999992, 160239.3830000013 },
        //                 new[] { 243245.9949999973, 160238.7989999987 },
        //                 new[] { 243261.3599999994, 160239.0 },
        //                 new[] { 243279.0160000026, 160244.1570000015 }
        //             }
        //         }
        //     },
        //     MethodeWegsegmentgeometrie = RoadSegmentGeometryDrawMethod.Outlined.ToDutchString(),
        //     BeginknoopObjectId = 287335,
        //     EindknoopObjectId = 287336,
        //     Linkerstraatnaam = new WegsegmentStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Rechterstraatnaam = new WegsegmentStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Wegsegmentstatus = RoadSegmentStatus.InUse.ToDutchString(),
        //     MorfologischeWegklasse = RoadSegmentMorphology.Road_consisting_of_one_roadway.ToDutchString(),
        //     Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.ToDutchString(),
        //     Wegbeheerder = "1304",
        //     Wegcategorie = RoadSegmentCategory.LocalRoadType3.ToDutchString(),
        //     Wegverharding = new[]
        //     {
        //         new WegverhardingObject
        //         {
        //             VanPositie = 0.000,
        //             TotPositie = 44.877,
        //             Verharding = RoadSegmentSurfaceType.LooseSurface.ToDutchString()
        //         }
        //     },
        //     Wegbreedte = new[]
        //     {
        //         new WegbreedteObject { VanPositie = 0.000, TotPositie = 11.526, Breedte = 6 },
        //         new WegbreedteObject { VanPositie = 11.526, TotPositie = 44.877, Breedte = 4 }
        //     },
        //     AantalRijstroken = new[]
        //     {
        //         new AantalRijstrokenObject
        //         {
        //             VanPositie = 0.000,
        //             TotPositie = 44.877,
        //             Aantal = 2,
        //             Richting = RoadSegmentLaneDirection.Independent.ToDutchString()
        //         }
        //     },
        //     EuropeseWegen = new[]
        //     {
        //         new EuropeseWegObject
        //         {
        //             EuNummer = "E40"
        //         }
        //     },
        //     NationaleWegen = new[]
        //     {
        //         new NationaleWegObject
        //         {
        //             Ident2 = "N180"
        //         }
        //     },
        //     GenummerdeWegen = new[]
        //     {
        //         new GenummerdeWegObject
        //         {
        //             Ident8 = "N0080001",
        //             Richting = RoadSegmentNumberedRoadDirection.Forward.ToDutchString(),
        //             Volgnummer = new RoadSegmentNumberedRoadOrdinal(2686).ToDutchString()
        //         }
        //     }
        // };
    }
}
