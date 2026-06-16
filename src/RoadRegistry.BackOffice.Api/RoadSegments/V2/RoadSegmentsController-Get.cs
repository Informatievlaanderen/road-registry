// namespace RoadRegistry.BackOffice.Api.RoadSegments.V2;
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.Serialization;
// using System.Threading;
// using System.Threading.Tasks;
// using Be.Vlaanderen.Basisregisters.Api.ETag;
// using Be.Vlaanderen.Basisregisters.Api.Exceptions;
// using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
// using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
// using Marten;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Options;
// using Newtonsoft.Json;
// using RoadRegistry.BackOffice.Abstractions.Exceptions;
// using RoadRegistry.BackOffice.Abstractions.RoadSegments;
// using RoadRegistry.BackOffice.Api.Infrastructure;
// using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
// using RoadRegistry.BackOffice.Api.Infrastructure.Options;
// using RoadRegistry.BackOffice.Extensions;
// using RoadRegistry.Read.Projections;
// using Swashbuckle.AspNetCore.Annotations;
// using Swashbuckle.AspNetCore.Filters;
//
// public partial class RoadSegmentsController
// {
//     private const string GetRoadSegmentRoute = "{id}";
//
//     /// <summary>
//     ///     Vraag een wegsegment op.
//     /// </summary>
//     /// <param name="id">De identificator van het wegsegment.</param>
//     /// <param name="responseOptions"></param>
//     /// <param name="store"></param>
//     /// <param name="cancellationToken"></param>
//     /// <response code="200">Als het wegsegment gevonden is.</response>
//     /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
//     /// <response code="410">Als het wegsegment is verwijderd.</response>
//     /// <response code="500">Als er een interne fout is opgetreden.</response>
//     [HttpGet(GetRoadSegmentRoute, Name = nameof(GetRoadSegment))]
//     [AllowAnonymous]
//     [ProducesResponseType(typeof(GetRoadSegmentResponseV2), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//     [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetRoadSegmentResponseResponseExamples))]
//     [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
//     [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
//     [SwaggerOperation(OperationId = nameof(GetRoadSegment), Description = "Attributen wijzigen van een wegsegment: status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie.")]
//     public async Task<IActionResult> GetRoadSegment(
//         [FromRoute] int id,
//         [FromServices] IOptions<ResponseOptions> responseOptions,
//         [FromServices] IDocumentStore store,
//         CancellationToken cancellationToken = default)
//     {
//         await using var session = store.LightweightSession();
//
//         var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id, cancellationToken);
//         if (roadSegment is null)
//         {
//             return NotFound();
//         }
//
//         if (roadSegment.IsRemoved)
//         {
//             return new StatusCodeResult(StatusCodes.Status410Gone);
//         }
//
//
//         var result = new GetRoadSegmentResponseV2
//         {
//             Identificator = new Identificator(responseOptions.Value.WegsegmentNaamruimte, roadSegment.RoadSegmentId.ToString(), roadSegment.Origin.Timestamp.ToDateTimeOffset()),
//             WegsegmentGeometrie = new WegsegmentGeometrie
//             {
//                 Geometrie = [new WegsegmentGeometrieProjectie
//                 {
//                     Gml = roadSegment.Geometry.Lambert08.Value.ToGeoJson() //TODO-pr serialize to GML
//                 }]
//             },
//             MethodeWegsegmentgeometrie = detailResponse.GeometryDrawMethod.Translation.Name,
//             BeginknoopObjectId = detailResponse.StartNodeId,
//             EindknoopObjectId = detailResponse.EndNodeId,
//             Linkerstraatnaam = detailResponse.LeftStreetNameId != null
//                 ? new WegsegmentStraatnaamObject
//                 {
//                     ObjectId = new StreetNameId(detailResponse.LeftStreetNameId.Value).ToString(),
//                     Straatnaam = detailResponse.LeftStreetName
//                 }
//                 : null,
//             Rechterstraatnaam = detailResponse.RightStreetNameId != null
//                 ? new WegsegmentStraatnaamObject
//                 {
//                     ObjectId = new StreetNameId(detailResponse.RightStreetNameId.Value).ToString(),
//                     Straatnaam = detailResponse.RightStreetName
//                 }
//                 : null,
//             Wegsegmentstatus = detailResponse.Status.Translation.Name,
//             MorfologischeWegklasse = detailResponse.Morphology.Translation.Name,
//             Toegangsbeperking = detailResponse.AccessRestriction.Translation.Name,
//             Wegbeheerder = detailResponse.MaintenanceAuthority.Code,
//             Wegcategorie = detailResponse.Category.Translation.Name,
//             Wegverharding = detailResponse.SurfaceTypes
//                 .Select(x => new WegverhardingObject
//                 {
//                     VanPositie = x.FromPosition,
//                     TotPositie = x.ToPosition,
//                     Verharding = x.SurfaceType.Translation.Name
//                 }).ToArray(),
//             Wegbreedte = detailResponse.Widths
//                 .Select(x => new WegbreedteObject
//                 {
//                     VanPositie = x.FromPosition,
//                     TotPositie = x.ToPosition,
//                     Breedte = x.Width
//                 }).ToArray(),
//             AantalRijstroken = detailResponse.LaneCounts
//                 .Select(x => new AantalRijstrokenObject
//                 {
//                     VanPositie = x.FromPosition,
//                     TotPositie = x.ToPosition,
//                     Aantal = x.Count,
//                     Richting = x.Direction.Translation.Name
//                 }).ToArray(),
//             EuropeseWegen = detailResponse.EuropeanRoads
//                 .Select(x => new EuropeseWegObject
//                 {
//                     EuNummer = x.Number
//                 }).ToArray(),
//             NationaleWegen = detailResponse.NationalRoads
//                 .Select(x => new NationaleWegObject
//                 {
//                     Ident2 = x.Number
//                 }).ToArray(),
//             GenummerdeWegen = detailResponse.NumberedRoads
//                 .Select(x => new GenummerdeWegObject
//                 {
//                     Ident8 = x.Number,
//                     Richting = x.Direction.Translation.Name,
//                     Volgnummer = x.Ordinal.ToDutchString()
//                 }).ToArray(),
//             Verwijderd = detailResponse.IsRemoved
//         };
//
//         return string.IsNullOrWhiteSpace(detailResponse.LastEventHash)
//             ? Ok(result)
//             : new OkWithLastObservedPositionAsETagResult(result, detailResponse.LastEventHash);
//     }
// }
//
// [DataContract(Name = "WegsegmentV2Detail", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentV2Detail")]
// public class GetRoadSegmentResponseV2
// {
//     [DataMember(Name = "Identificator", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public Identificator Identificator { get; set; }
//
//     /// <summary>
//     ///     De middellijngeometrie van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "wegsegmentGeometrie", Order = 2)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public WegsegmentGeometrie WegsegmentGeometrie { get; set; }
//
//     /// <summary>
//     ///     De geometriemethode van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "MethodeWegsegmentgeometrie", Order = 3)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentGeometryDrawMethod))]
//     public string MethodeWegsegmentgeometrie { get; set; }
//
//     /// <summary>
//     ///     De identificator van het beginknoop object.
//     /// </summary>
//     [DataMember(Name = "BeginknoopObjectId", Order = 4)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public int BeginknoopObjectId { get; set; }
//
//     /// <summary>
//     ///     De identificator van het eindknoop object.
//     /// </summary>
//     [DataMember(Name = "EindknoopObjectId", Order = 5)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public int EindknoopObjectId { get; set; }
//
//     [DataMember(Name = "Linkerstraatnaam", Order = 6)]
//     [JsonProperty]
//     public WegsegmentStraatnaamObject Linkerstraatnaam { get; set; }
//
//     [DataMember(Name = "Rechterstraatnaam", Order = 7)]
//     [JsonProperty]
//     public WegsegmentStraatnaamObject Rechterstraatnaam { get; set; }
//
//     /// <summary>
//     ///     De status van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Wegsegmentstatus", Order = 8)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentStatus))]
//     public string Wegsegmentstatus { get; set; }
//
//     /// <summary>
//     ///     De morfologische wegklasse van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "MorfologischeWegklasse", Order = 9)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentMorphology))]
//     public string MorfologischeWegklasse { get; set; }
//
//     /// <summary>
//     ///     De toegangsbeperking van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Toegangsbeperking", Order = 10)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestriction))]
//     public string Toegangsbeperking { get; set; }
//
//     /// <summary>
//     ///     De wegbeheerder van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Wegbeheerder", Order = 11)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public string Wegbeheerder { get; set; }
//
//     /// <summary>
//     ///     De wegcategorie van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Wegcategorie", Order = 12)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentCategory))]
//     public string Wegcategorie { get; set; }
//
//     /// <summary>
//     ///     De wegverharding van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Wegverharding", Order = 13)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public WegverhardingObject[] Wegverharding { get; set; }
//
//     /// <summary>
//     ///     De wegbreedte van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Wegbreedte", Order = 14)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public WegbreedteObject[] Wegbreedte { get; set; }
//
//     /// <summary>
//     ///     Het aantal rijstroken van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "AantalRijstroken", Order = 15)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public AantalRijstrokenObject[] AantalRijstroken { get; set; }
//
//     /// <summary>
//     ///     De gekoppelde Europese wegen van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "EuropeseWegen", Order = 16)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public EuropeseWegObject[] EuropeseWegen { get; set; }
//
//     /// <summary>
//     ///     De gekoppelde nationale wegen van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "NationaleWegen", Order = 17)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public NationaleWegObject[] NationaleWegen { get; set; }
//
//     /// <summary>
//     ///     De gekoppelde genummerde wegen van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "GenummerdeWegen", Order = 18)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public GenummerdeWegObject[] GenummerdeWegen { get; set; }
//
//     /// <summary>
//     ///     Geeft aan of het wegsegment al dan niet verwijderd werd.
//     /// </summary>
//     [DataMember(Name = "Verwijderd", Order = 99)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public bool Verwijderd { get; set; }
// }
//
// [DataContract(Name = "Straatnaam", Namespace = "")]
// [CustomSwaggerSchemaId("Straatnaam")]
// public class StraatnaamObject
// {
//     /// <summary>
//     /// De objectidentificator van de straatnaam.
//     /// </summary>
//     [DataMember(Name = "ObjectId", Order = 1)]
//     [JsonProperty]
//     public string ObjectId { get; set; }
//
//     /// <summary>
//     /// De naam van de straatnaam.
//     /// </summary>
//     [DataMember(Name = "Straatnaam", Order = 2)]
//     [JsonProperty]
//     public string Straatnaam { get; set; }
// }
//
// [DataContract(Name = "WegsegmentV2Geometrie", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentV2Geometrie")]
// public class WegsegmentGeometrie
// {
//     /// <summary>
//     /// De objectidentificator van de straatnaam.
//     /// </summary>
//     [DataMember(Name = "Geometrie", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public IReadOnlyCollection<WegsegmentGeometrieProjectie> Geometrie { get; set; }
//
//     /// <summary>
//     /// GeoJSON-geometrietype.
//     /// </summary>
//     [DataMember(Name = "Type", Order = 2)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public string Type { get; } = "LineString";
// }
//
// [DataContract(Name = "WegsegmentV2GeometrieProjectie", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentV2GeometrieProjectie")]
// public class WegsegmentGeometrieProjectie
// {
//     [DataMember(Name = "gml", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public string Gml { get; set; }
// }
//
// /// <summary>
// ///     Bevat informatie over de straatnaam van het wegsegment.
// /// </summary>
// [DataContract(Name = "WegsegmentStraatnaamObject", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentStraatnaamObject")]
// public class WegsegmentStraatnaamObject : StraatnaamObject
// {
// }
//
// [DataContract(Name = "WegsegmentWegverharding", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentWegverharding")]
// public class WegverhardingObject
// {
//     /// <summary>
//     /// Van positie.
//     /// </summary>
//     [DataMember(Name = "VanPositie", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double VanPositie { get; set; }
//
//     /// <summary>
//     /// Tot positie.
//     /// </summary>
//     [DataMember(Name = "TotPositie", Order = 2)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double TotPositie { get; set; }
//
//     /// <summary>
//     /// Type wegverharding van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Verharding", Order = 3)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceType))]
//     public string Verharding { get; set; }
// }
//
// [DataContract(Name = "WegsegmentWegbreedte", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentWegbreedte")]
// public class WegbreedteObject
// {
//     /// <summary>
//     /// Van positie.
//     /// </summary>
//     [DataMember(Name = "VanPositie", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double VanPositie { get; set; }
//
//     /// <summary>
//     /// Tot positie.
//     /// </summary>
//     [DataMember(Name = "TotPositie", Order = 2)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double TotPositie { get; set; }
//
//     /// <summary>
//     /// Breedte van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Breedte", Order = 3)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public int Breedte { get; set; }
// }
//
// [DataContract(Name = "WegsegmentAantalRijstroken", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentAantalRijstroken")]
// public class AantalRijstrokenObject
// {
//     /// <summary>
//     /// Van positie.
//     /// </summary>
//     [DataMember(Name = "VanPositie", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double VanPositie { get; set; }
//
//     /// <summary>
//     /// Tot positie.
//     /// </summary>
//     [DataMember(Name = "TotPositie", Order = 2)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public double TotPositie { get; set; }
//
//     /// <summary>
//     /// Aantal rijstroken van het wegsegment.
//     /// </summary>
//     [DataMember(Name = "Aantal", Order = 3)]
//     [JsonProperty]
//     public int Aantal { get; set; }
//
//     /// <summary>
//     /// Richting t.o.v. de richting van het wegsegment (begin- naar eindknoop).
//     /// </summary>
//     [DataMember(Name = "Richting", Order = 4)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentLaneDirection))]
//     public string Richting { get; set; }
// }
//
// [DataContract(Name = "WegsegmentEuropeseWeg", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentEuropeseWeg")]
// public class EuropeseWegObject
// {
//     /// <summary>
//     /// Nummer van de Europese weg.
//     /// </summary>
//     [DataMember(Name = "EuNummer", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(EuropeanRoadNumber))]
//     public string EuNummer { get; set; }
// }
//
// [DataContract(Name = "WegsegmentNationaleWeg", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentNationaleWeg")]
// public class NationaleWegObject
// {
//     /// <summary>
//     /// Ident2 van de nationale weg.
//     /// </summary>
//     [DataMember(Name = "Ident2", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public string Ident2 { get; set; }
// }
//
// [DataContract(Name = "WegsegmentGenummerdeWeg", Namespace = "")]
// [CustomSwaggerSchemaId("WegsegmentGenummerdeWeg")]
// public class GenummerdeWegObject
// {
//     /// <summary>
//     /// Ident8 van de genummerde weg.
//     /// </summary>
//     [DataMember(Name = "Ident8", Order = 1)]
//     [JsonProperty(Required = Required.DisallowNull)]
//     public string Ident8 { get; set; }
//
//     /// <summary>
//     ///     Richting van de genummerde weg.
//     /// </summary>
//     [DataMember(Name = "Richting", Order = 2)]
//     [JsonProperty("richting", Required = Required.DisallowNull)]
//     [RoadRegistryEnumDataType(typeof(RoadSegmentNumberedRoadDirection))]
//     public string Richting { get; set; }
//
//     /// <summary>
//     ///     Volgnummer van de genummerde weg (geheel positief getal of "niet gekend").
//     /// </summary>
//     [DataMember(Name = "Volgnummer", Order = 3)]
//     [JsonProperty("volgnummer", Required = Required.DisallowNull)]
//     public string Volgnummer { get; set; }
// }
//
// public class GetRoadSegmentResponseResponseExamples : IExamplesProvider<GetRoadSegmentResponseV2>
// {
//     public GetRoadSegmentResponseV2 GetExamples()
//     {
//         return new GetRoadSegmentResponseV2
//         {
//             Identificator = new WegsegmentIdentificatorV2("https://data.vlaanderen.be/id/wegsegment", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
//             MiddellijnGeometrie = new GeoJSONMultiLineString
//             {
//                 Coordinates = new[]
//                 {
//                     new[]
//                     {
//                         new[] { 243234.8929999992, 160239.3830000013 },
//                         new[] { 243245.9949999973, 160238.7989999987 },
//                         new[] { 243261.3599999994, 160239.0 },
//                         new[] { 243279.0160000026, 160244.1570000015 }
//                     }
//                 }
//             },
//             MethodeWegsegmentgeometrie = RoadSegmentGeometryDrawMethod.Outlined.ToDutchString(),
//             BeginknoopObjectId = 287335,
//             EindknoopObjectId = 287336,
//             Linkerstraatnaam = new WegsegmentStraatnaamObject
//             {
//                 ObjectId = new StreetNameId(71671).ToString(),
//                 Straatnaam = "Smidsestraat"
//             },
//             Rechterstraatnaam = new WegsegmentStraatnaamObject
//             {
//                 ObjectId = new StreetNameId(71671).ToString(),
//                 Straatnaam = "Smidsestraat"
//             },
//             Wegsegmentstatus = RoadSegmentStatus.InUse.ToDutchString(),
//             MorfologischeWegklasse = RoadSegmentMorphology.Road_consisting_of_one_roadway.ToDutchString(),
//             Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.ToDutchString(),
//             Wegbeheerder = "1304",
//             Wegcategorie = RoadSegmentCategory.LocalRoadType3.ToDutchString(),
//             Wegverharding = new[]
//             {
//                 new WegverhardingObject
//                 {
//                     VanPositie = 0.000,
//                     TotPositie = 44.877,
//                     Verharding = RoadSegmentSurfaceType.LooseSurface.ToDutchString()
//                 }
//             },
//             Wegbreedte = new[]
//             {
//                 new WegbreedteObject { VanPositie = 0.000, TotPositie = 11.526, Breedte = 6 },
//                 new WegbreedteObject { VanPositie = 11.526, TotPositie = 44.877, Breedte = 4 }
//             },
//             AantalRijstroken = new[]
//             {
//                 new AantalRijstrokenObject
//                 {
//                     VanPositie = 0.000,
//                     TotPositie = 44.877,
//                     Aantal = 2,
//                     Richting = RoadSegmentLaneDirection.Independent.ToDutchString()
//                 }
//             },
//             EuropeseWegen = new[]
//             {
//                 new EuropeseWegObject
//                 {
//                     EuNummer = "E40"
//                 }
//             },
//             NationaleWegen = new[]
//             {
//                 new NationaleWegObject
//                 {
//                     Ident2 = "N180"
//                 }
//             },
//             GenummerdeWegen = new[]
//             {
//                 new GenummerdeWegObject
//                 {
//                     Ident8 = "N0080001",
//                     Richting = RoadSegmentNumberedRoadDirection.Forward.ToDutchString(),
//                     Volgnummer = new RoadSegmentNumberedRoadOrdinal(2686).ToDutchString()
//                 }
//             }
//         };
//     }
// }
