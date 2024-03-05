namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
using Extensions;
using Infrastructure.Controllers.Attributes;
using Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadSegmentsController
{
    private const string GetRoadSegmentRoute = "{id}";

    /// <summary>
    ///     Vraag een wegsegment op.
    /// </summary>
    /// <param name="id">De identificator van het wegsegment.</param>
    /// <param name="responseOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetRoadSegmentRoute, Name = nameof(GetRoadSegment))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetRoadSegmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetRoadSegmentResponseResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetRoadSegment), Description = "Attributen wijzigen van een wegsegment: status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie.")]
    public async Task<IActionResult> GetRoadSegment(
        [FromRoute] int id,
        [FromServices] IOptions<ResponseOptions> responseOptions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var detailResponse = await _mediator.Send(new RoadSegmentDetailRequest(id), cancellationToken);

            var geoJsonGeometry = GeometryTranslator.Translate(detailResponse.Geometry);

            var result = new GetRoadSegmentResponse
            {
                Identificator = new WegsegmentIdentificator(responseOptions.Value.WegsegmentNaamruimte, detailResponse.RoadSegmentId.ToString(), detailResponse.BeginTime, detailResponse.Version),
                MiddellijnGeometrie = new GeoJSONMultiLineString
                {
                    Coordinates = geoJsonGeometry.ToGeoJson().ToCoordinateArray()
                },
                MethodeWegsegmentgeometrie = detailResponse.GeometryDrawMethod.Translation.Name,
                BeginknoopObjectId = detailResponse.StartNodeId,
                EindknoopObjectId = detailResponse.EndNodeId,
                Linkerstraatnaam = detailResponse.LeftStreetNameId != null
                    ? new WegsegmentStraatnaamObject
                    {
                        ObjectId = new StreetNamePuri(detailResponse.LeftStreetNameId.Value).ToString(),
                        Straatnaam = detailResponse.LeftStreetName
                    }
                    : null,
                Rechterstraatnaam = detailResponse.RightStreetNameId != null
                    ? new WegsegmentStraatnaamObject
                    {
                        ObjectId = new StreetNamePuri(detailResponse.RightStreetNameId.Value).ToString(),
                        Straatnaam = detailResponse.RightStreetName
                    }
                    : null,
                Wegsegmentstatus = detailResponse.Status.Translation.Name,
                MorfologischeWegklasse = detailResponse.Morphology.Translation.Name,
                Toegangsbeperking = detailResponse.AccessRestriction.Translation.Name,
                Wegbeheerder = detailResponse.MaintenanceAuthority.Code,
                Wegcategorie = detailResponse.Category.Translation.Name,
                Wegverharding = detailResponse.SurfaceTypes
                    .Select(x => new WegverhardingObject
                    {
                        VanPositie = x.FromPosition,
                        TotPositie = x.ToPosition,
                        Verharding = x.SurfaceType.Translation.Name
                    }).ToArray(),
                Wegbreedte = detailResponse.Widths
                    .Select(x => new WegbreedteObject
                    {
                        VanPositie = x.FromPosition,
                        TotPositie = x.ToPosition,
                        Breedte = x.Width
                    }).ToArray(),
                AantalRijstroken = detailResponse.LaneCounts
                    .Select(x => new AantalRijstrokenObject
                    {
                        VanPositie = x.FromPosition,
                        TotPositie = x.ToPosition,
                        Aantal = x.Count,
                        Richting = x.Direction.Translation.Name
                    }).ToArray(),
                EuropeseWegen = detailResponse.EuropeanRoads
                    .Select(x => new EuropeseWegObject
                    {
                        EuNummer = x.Number
                    }).ToArray(),
                NationaleWegen = detailResponse.NationalRoads
                    .Select(x => new NationaleWegObject
                    {
                        Ident2 = x.Number
                    }).ToArray(),
                GenummerdeWegen = detailResponse.NumberedRoads
                    .Select(x => new GenummerdeWegObject
                    {
                        Ident8 = x.Number,
                        Richting = x.Direction.Translation.Name,
                        Volgnummer = x.Ordinal.ToDutchString()
                    }).ToArray(),
                Verwijderd = detailResponse.IsRemoved
            };

            return string.IsNullOrWhiteSpace(detailResponse.LastEventHash)
                ? Ok(result)
                : new OkWithLastObservedPositionAsETagResult(result, detailResponse.LastEventHash);
        }
        catch (RoadSegmentNotFoundException)
        {
            return NotFound();
        }
    }
}

[DataContract(Name = "WegsegmentDetail", Namespace = "")]
public class GetRoadSegmentResponse
{
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegsegmentIdentificator Identificator { get; set; }

    /// <summary>
    ///     De middellijngeometrie van het wegsegment.
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public GeoJSONMultiLineString MiddellijnGeometrie { get; set; }

    /// <summary>
    ///     De geometriemethode van het wegsegment.
    /// </summary>
    [DataMember(Name = "MethodeWegsegmentgeometrie", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentGeometryDrawMethod))]
    public string MethodeWegsegmentgeometrie { get; set; }

    /// <summary>
    ///     De identificator van het beginknoop object.
    /// </summary>
    [DataMember(Name = "BeginknoopObjectId", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public int BeginknoopObjectId { get; set; }

    /// <summary>
    ///     De identificator van het eindknoop object.
    /// </summary>
    [DataMember(Name = "EindknoopObjectId", Order = 5)]
    [JsonProperty(Required = Required.DisallowNull)]
    public int EindknoopObjectId { get; set; }

    [DataMember(Name = "Linkerstraatnaam", Order = 6)]
    [JsonProperty]
    public WegsegmentStraatnaamObject Linkerstraatnaam { get; set; }

    [DataMember(Name = "Rechterstraatnaam", Order = 7)]
    [JsonProperty]
    public WegsegmentStraatnaamObject Rechterstraatnaam { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 8)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatus))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     De morfologische wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 9)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphology))]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegangsbeperking van het wegsegment.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 10)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestriction))]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De wegbeheerder van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 11)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     De wegcategorie van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 12)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentCategory))]
    public string Wegcategorie { get; set; }

    /// <summary>
    ///     De wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 13)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegverhardingObject[] Wegverharding { get; set; }

    /// <summary>
    ///     De wegbreedte van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegbreedte", Order = 14)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegbreedteObject[] Wegbreedte { get; set; }

    /// <summary>
    ///     Het aantal rijstroken van het wegsegment.
    /// </summary>
    [DataMember(Name = "AantalRijstroken", Order = 15)]
    [JsonProperty(Required = Required.DisallowNull)]
    public AantalRijstrokenObject[] AantalRijstroken { get; set; }

    /// <summary>
    ///     De gekoppelde Europese wegen van het wegsegment.
    /// </summary>
    [DataMember(Name = "EuropeseWegen", Order = 16)]
    [JsonProperty(Required = Required.DisallowNull)]
    public EuropeseWegObject[] EuropeseWegen { get; set; }

    /// <summary>
    ///     De gekoppelde nationale wegen van het wegsegment.
    /// </summary>
    [DataMember(Name = "NationaleWegen", Order = 17)]
    [JsonProperty(Required = Required.DisallowNull)]
    public NationaleWegObject[] NationaleWegen { get; set; }

    /// <summary>
    ///     De gekoppelde genummerde wegen van het wegsegment.
    /// </summary>
    [DataMember(Name = "GenummerdeWegen", Order = 18)]
    [JsonProperty(Required = Required.DisallowNull)]
    public GenummerdeWegObject[] GenummerdeWegen { get; set; }

    /// <summary>
    ///     Geeft aan of het wegsegment al dan niet verwijderd werd.
    /// </summary>
    [DataMember(Name = "Verwijderd", Order = 99)]
    [JsonProperty(Required = Required.DisallowNull)]
    public bool Verwijderd { get; set; }
}

/// <summary>
///     Bevat informatie waarmee het wegsegment kan geïdentificeerd worden.
/// </summary>
[DataContract(Name = "WegsegmentIdentificator", Namespace = "")]
public class WegsegmentIdentificator : Identificator
{
    /// <summary>
    /// Het versie nummer van het object.
    /// </summary>
    [DataMember(Name = "VersieNummer", Order = 5)]
    [JsonProperty(Required = Required.DisallowNull)]
    public int VersieNummer { get; set; }

    public WegsegmentIdentificator(string naamruimte, string objectId, DateTimeOffset? versie, int versieNummer)
        : base(naamruimte, objectId, versie)
    {
        VersieNummer = versieNummer;
    }

    public WegsegmentIdentificator()
        : base(null, null, (string)null)
    {
    }
}

[DataContract(Name = "Straatnaam", Namespace = "")]
public class StraatnaamObject
{
    /// <summary>
    /// De objectidentificator van de straatnaam.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    /// <summary>
    /// De naam van de straatnaam.
    /// </summary>
    [DataMember(Name = "Straatnaam", Order = 2)]
    [JsonProperty]
    public string Straatnaam { get; set; }
}

/// <summary>
///     Bevat informatie over de straatnaam van het wegsegment.
/// </summary>
[DataContract(Name = "WegsegmentStraatnaamObject", Namespace = "")]
public class WegsegmentStraatnaamObject : StraatnaamObject
{
}

[DataContract(Name = "WegsegmentWegverharding", Namespace = "")]
public class WegverhardingObject
{
    /// <summary>
    /// Van positie.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double VanPositie { get; set; }

    /// <summary>
    /// Tot positie.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double TotPositie { get; set; }

    /// <summary>
    /// Type wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Verharding", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceType))]
    public string Verharding { get; set; }
}

[DataContract(Name = "WegsegmentWegbreedte", Namespace = "")]
public class WegbreedteObject
{
    /// <summary>
    /// Van positie.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double VanPositie { get; set; }

    /// <summary>
    /// Tot positie.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double TotPositie { get; set; }

    /// <summary>
    /// Breedte van het wegsegment.
    /// </summary>
    [DataMember(Name = "Breedte", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public int Breedte { get; set; }
}

[DataContract(Name = "WegsegmentAantalRijstroken", Namespace = "")]
public class AantalRijstrokenObject
{
    /// <summary>
    /// Van positie.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double VanPositie { get; set; }

    /// <summary>
    /// Tot positie.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public double TotPositie { get; set; }

    /// <summary>
    /// Aantal rijstroken van het wegsegment.
    /// </summary>
    [DataMember(Name = "Aantal", Order = 3)]
    [JsonProperty]
    public int Aantal { get; set; }

    /// <summary>
    /// Richting t.o.v. de richting van het wegsegment (begin- naar eindknoop).
    /// </summary>
    [DataMember(Name = "Richting", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentLaneDirection))]
    public string Richting { get; set; }
}

[DataContract(Name = "WegsegmentEuropeseWeg", Namespace = "")]
public class EuropeseWegObject
{
    /// <summary>
    /// Nummer van de Europese weg.
    /// </summary>
    [DataMember(Name = "EuNummer", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(EuropeanRoadNumber))]
    public string EuNummer { get; set; }
}

[DataContract(Name = "WegsegmentNationaleWeg", Namespace = "")]
public class NationaleWegObject
{
    /// <summary>
    /// Ident2 van de nationale weg.
    /// </summary>
    [DataMember(Name = "Ident2", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Ident2 { get; set; }
}

[DataContract(Name = "WegsegmentGenummerdeWeg", Namespace = "")]
public class GenummerdeWegObject
{
    /// <summary>
    /// Ident8 van de genummerde weg.
    /// </summary>
    [DataMember(Name = "Ident8", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Ident8 { get; set; }

    /// <summary>
    ///     Richting van de genummerde weg.
    /// </summary>
    [DataMember(Name = "Richting", Order = 2)]
    [JsonProperty("richting", Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentNumberedRoadDirection))]
    public string Richting { get; set; }

    /// <summary>
    ///     Volgnummer van de genummerde weg (geheel positief getal of "niet gekend").
    /// </summary>
    [DataMember(Name = "Volgnummer", Order = 3)]
    [JsonProperty("volgnummer", Required = Required.DisallowNull)]
    public string Volgnummer { get; set; }
}

public class GetRoadSegmentResponseResponseExamples : IExamplesProvider<GetRoadSegmentResponse>
{
    public GetRoadSegmentResponse GetExamples()
    {
        return new GetRoadSegmentResponse
        {
            Identificator = new WegsegmentIdentificator("https://data.vlaanderen.be/id/wegsegment", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
            MiddellijnGeometrie = new GeoJSONMultiLineString
            {
                Coordinates = new[]
                {
                    new[]
                    {
                        new[] { 243234.8929999992, 160239.3830000013 },
                        new[] { 243245.9949999973, 160238.7989999987 },
                        new[] { 243261.3599999994, 160239.0 },
                        new[] { 243279.0160000026, 160244.1570000015 }
                    }
                }
            },
            MethodeWegsegmentgeometrie = RoadSegmentGeometryDrawMethod.Outlined.Translation.Name,
            BeginknoopObjectId = 287335,
            EindknoopObjectId = 287336,
            Linkerstraatnaam = new WegsegmentStraatnaamObject
            {
                ObjectId = new StreetNamePuri(71671).ToString(),
                Straatnaam = "Smidsestraat"
            },
            Rechterstraatnaam = new WegsegmentStraatnaamObject
            {
                ObjectId = new StreetNamePuri(71671).ToString(),
                Straatnaam = "Smidsestraat"
            },
            Wegsegmentstatus = RoadSegmentStatus.InUse.Translation.Name,
            MorfologischeWegklasse = RoadSegmentMorphology.Road_consisting_of_one_roadway.Translation.Name,
            Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.Translation.Name,
            Wegbeheerder = "1304",
            Wegcategorie = RoadSegmentCategory.LocalRoadType3.Translation.Name,
            Wegverharding = new[]
            {
                new WegverhardingObject
                {
                    VanPositie = 0.000,
                    TotPositie = 44.877,
                    Verharding = RoadSegmentSurfaceType.LooseSurface.Translation.Name
                }
            },
            Wegbreedte = new[]
            {
                new WegbreedteObject { VanPositie = 0.000, TotPositie = 11.526, Breedte = 6 },
                new WegbreedteObject { VanPositie = 11.526, TotPositie = 44.877, Breedte = 4 }
            },
            AantalRijstroken = new[]
            {
                new AantalRijstrokenObject
                {
                    VanPositie = 0.000,
                    TotPositie = 44.877,
                    Aantal = 2,
                    Richting = RoadSegmentLaneDirection.Independent.Translation.Name
                }
            }
        };
    }
}
