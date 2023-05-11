namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string GetRoadSegmentRoute = "{id}";

    /// <summary>
    ///     Vraag een wegsegment op.
    /// </summary>
    /// <param name="id">De identificator van het wegsegment.</param>
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var detailResponse = await _mediator.Send(new RoadSegmentDetailRequest(id), cancellationToken);

            var geoJsonGeometry = GeometryTranslator.Translate(detailResponse.Geometry);

            var result = new GetRoadSegmentResponse
            {
                Identificator = new Identificator
                {
                    ObjectId = detailResponse.RoadSegmentId.ToString(),
                    VersieId = detailResponse.BeginTime
                },
                MiddellijnGeometrie = GeometryTranslator.ToGeoJson(geoJsonGeometry).ToString(),
                MethodeWegsegmentgeometrie = detailResponse.GeometryDrawMethod.Translation.Name,
                BeginknoopObjectId = detailResponse.StartNodeId,
                EindknoopObjectId = detailResponse.EndNodeId,
                Linkerstraatnaam = detailResponse.LeftStreetNameId != null
                    ? new StraatnaamObject
                    {
                        ObjectId = new StreetNamePuri(detailResponse.LeftStreetNameId.Value).ToString(),
                        Straatnaam = detailResponse.LeftStreetName
                    }
                    : null,
                Rechterstraatnaam = detailResponse.RightStreetNameId != null
                    ? new StraatnaamObject
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
                    .Select(s => new WegverhardingObject
                    {
                        VanPositie = s.FromPosition,
                        TotPositie = s.ToPosition,
                        Verharding = s.SurfaceType.Translation.Name
                    }).ToArray(),
                Wegbreedte = detailResponse.Widths
                    .Select(s => new WegbreedteObject
                    {
                        VanPositie = s.FromPosition,
                        TotPositie = s.ToPosition,
                        Breedte = s.Width
                    }).ToArray(),
                AantalRijstroken = detailResponse.LaneCounts
                    .Select(s => new AantalRijstrokenObject
                    {
                        VanPositie = s.FromPosition,
                        TotPositie = s.ToPosition,
                        Aantal = s.Count,
                        Richting = s.Direction.Translation.Name
                    }).ToArray()
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

[DataContract(Name = "Identificator", Namespace = "")]
public class Identificator
{
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    [DataMember(Name = "VersieId", Order = 2)]
    [JsonProperty]
    public DateTime VersieId { get; set; }
}

[DataContract(Name = "Straatnaam", Namespace = "")]
public class StraatnaamObject
{
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    [DataMember(Name = "Straatnaam", Order = 2)]
    [JsonProperty]
    public string Straatnaam { get; set; }
}

[DataContract(Name = "Wegverharding", Namespace = "")]
public class WegverhardingObject
{
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty]
    public double VanPositie { get; set; }

    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty]
    public double TotPositie { get; set; }

    [DataMember(Name = "Verharding", Order = 3)]
    [JsonProperty]
    public string Verharding { get; set; }
}

[DataContract(Name = "Wegbreedte", Namespace = "")]
public class WegbreedteObject
{
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty]
    public double VanPositie { get; set; }

    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty]
    public double TotPositie { get; set; }

    [DataMember(Name = "Breedte", Order = 3)]
    [JsonProperty]
    public int Breedte { get; set; }
}

[DataContract(Name = "AantalRijstroken", Namespace = "")]
public class AantalRijstrokenObject
{
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty]
    public double VanPositie { get; set; }

    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty]
    public double TotPositie { get; set; }

    [DataMember(Name = "Aantal", Order = 3)]
    [JsonProperty]
    public int Aantal { get; set; }

    [DataMember(Name = "Richting", Order = 4)]
    [JsonProperty]
    public string Richting { get; set; }
}

[DataContract(Name = "WegsegmentDetail", Namespace = "")]
public class GetRoadSegmentResponse
{
    /// <summary>
    ///     De unieke identificator van het wegsegment.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty]
    public Identificator Identificator { get; set; }

    /// <summary>
    ///     De middellijngeometrie van het wegsegment.
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 2)]
    [JsonProperty]
    public string MiddellijnGeometrie { get; set; }

    /// <summary>
    ///     De geometriemethode van het wegsegment.
    /// </summary>
    [DataMember(Name = "MethodeWegsegmentgeometrie", Order = 3)]
    [JsonProperty]
    public string MethodeWegsegmentgeometrie { get; set; }

    /// <summary>
    ///     De identificator van het beginknoop object.
    /// </summary>
    [DataMember(Name = "BeginknoopObjectId", Order = 4)]
    [JsonProperty]
    public int BeginknoopObjectId { get; set; }

    /// <summary>
    ///     De identificator van het eindknoop object.
    /// </summary>
    [DataMember(Name = "EindknoopObjectId", Order = 5)]
    [JsonProperty]
    public int EindknoopObjectId { get; set; }

    /// <summary>
    ///     De identificator van de linkerstraatnaam.
    /// </summary>
    [DataMember(Name = "Linkerstraatnaam", Order = 6)]
    [JsonProperty]
    public StraatnaamObject Linkerstraatnaam { get; set; }

    /// <summary>
    ///     De identificator van de rechterstraatnaam.
    /// </summary>
    [DataMember(Name = "Rechterstraatnaam", Order = 7)]
    [JsonProperty]
    public StraatnaamObject Rechterstraatnaam { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 8)]
    [JsonProperty]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     De morfologische wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 9)]
    [JsonProperty]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegangsbeperking van het wegsegment.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 10)]
    [JsonProperty]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De wegbeheerder van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 11)]
    [JsonProperty]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     De wegcategorie van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 12)]
    [JsonProperty]
    public string Wegcategorie { get; set; }

    /// <summary>
    ///     De wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 13)]
    [JsonProperty]
    public WegverhardingObject[] Wegverharding { get; set; }

    /// <summary>
    ///     De wegbreedte van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegbreedte", Order = 14)]
    [JsonProperty]
    public WegbreedteObject[] Wegbreedte { get; set; }

    /// <summary>
    ///     Het aantal rijstroken van het wegsegment.
    /// </summary>
    [DataMember(Name = "AantalRijstroken", Order = 15)]
    [JsonProperty]
    public AantalRijstrokenObject[] AantalRijstroken { get; set; }
}

public class GetRoadSegmentResponseResponseExamples : IExamplesProvider<GetRoadSegmentResponse>
{
    public GetRoadSegmentResponse GetExamples()
    {
        return new GetRoadSegmentResponse
        {
            Identificator = new Identificator
            {
                ObjectId = "643556",
                VersieId = new DateTime(2015, 11, 27, 13, 46, 14)
            },
            MiddellijnGeometrie = new MultiLineString(new[]
            {
                new[]
                {
                    new[] { 243234.8929999992, 160239.3830000013 },
                    new[] { 243245.9949999973, 160238.7989999987 },
                    new[] { 243261.3599999994, 160239.0 },
                    new[] { 243279.0160000026, 160244.1570000015 }
                }
            }).ToString(),
            MethodeWegsegmentgeometrie = RoadSegmentGeometryDrawMethod.Outlined.Translation.Name,
            BeginknoopObjectId = 287335,
            EindknoopObjectId = 287336,
            Linkerstraatnaam = new StraatnaamObject
            {
                ObjectId = new StreetNamePuri(71671).ToString(),
                Straatnaam = "Smidsestraat"
            },
            Rechterstraatnaam = new StraatnaamObject
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
