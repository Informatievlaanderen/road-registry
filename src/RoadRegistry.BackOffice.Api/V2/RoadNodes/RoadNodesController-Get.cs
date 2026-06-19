namespace RoadRegistry.BackOffice.Api.V2.RoadNodes;

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
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadNodesController
{
    private const string GetRoadNodeRoute = "{id}";

    /// <summary>
    ///     Vraag een wegknoop op.
    /// </summary>
    /// <param name="id">De identificator van het wegknoop.</param>
    /// <param name="responseOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegknoop gevonden is.</response>
    /// <response code="404">Als het wegknoop niet gevonden kan worden.</response>
    /// <response code="410">Als het wegknoop is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetRoadNodeRoute, Name = nameof(GetRoadNodeV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WegknoopV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(WegknoopV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadNodeNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(RoadNodeGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetRoadNodeV2), Description = "Vraag een wegknoop op.")]
    public async Task<IActionResult> GetRoadNodeV2(
        [FromRoute] int id,
        [FromServices] IOptions<ResponseOptions> responseOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var roadNode = await session.LoadAsync<RoadNodeReadItem>(id, cancellationToken);
        if (roadNode is null)
        {
            return NotFound();
        }

        if (roadNode.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new WegknoopV2Detail
        {
            Identificator = new Identificator(responseOptions.Value.WegknoopNaamruimte, roadNode.RoadNodeId.ToString(), roadNode.Origin.Timestamp.ToDateTimeOffset()),
            WegknoopGeometrie = new WegknoopGeometrie
            {
                Geometrie =
                [
                    new WegknoopGeometrieProjectie
                    {
                        Gml = roadNode.Geometry.Lambert08.Value.ConvertToGml()
                    },
                    new WegknoopGeometrieProjectie
                    {
                        Gml = roadNode.Geometry.Lambert72.Value.ConvertToGml()
                    }
                ]
            },
            AansluitendeWegsegmenten = roadNode.RoadSegmentIds
                .Select(x => new WegsegmentLink(x, responseOptions.Value.WegknoopDetailUrlFormat))
                .ToArray()
        };

        //TODO-pr to confirm what to return with V1 data
        if (roadNode.IsV2)
        {
            result.WegknoopType = RoadNodeTypeV2.Parse(roadNode.Type!).ToDutchString();
            result.Grensknoop = roadNode.Grensknoop;
        }

        return Ok(result);
    }
}

[DataContract(Name = "WegknoopV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopV2Detail")]
public class WegknoopV2Detail
{
    /// <summary>
    ///     Identificerende gegevens van de wegknoop.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required Identificator Identificator { get; set; }

    /// <summary>
    ///     De middellijngeometrie van het wegknoop.
    /// </summary>
    [DataMember(Name = "wegknoopGeometrie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegknoopGeometrie WegknoopGeometrie { get; set; }

    /// <summary>
    ///     De geometriemethode van het wegknoop.
    /// </summary>
    [DataMember(Name = "WegknoopType", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadNodeTypeV2))]
    public string WegknoopType { get; set; }

    /// <summary>
    ///     Een wegknoop is een grensknoop indien de wegknoop aansluit op wegen die buiten het Vlaamse gewest liggen.
    /// </summary>
    [DataMember(Name = "Grensknoop", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public bool Grensknoop { get; set; }

    /// <summary>
    ///     Geeft aan met welke gerealiseerde wegsegmenten de wegknoop verbonden is.
    /// </summary>
    [DataMember(Name = "AansluitendeWegsegmenten", Order = 5)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentLink> AansluitendeWegsegmenten { get; set; } = [];
}

[DataContract(Name = "WegknoopGeometrie", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopGeometrie")]
public class WegknoopGeometrie
{
    /// <summary>
    /// Geometrie van de wegknoop.
    /// </summary>
    [DataMember(Name = "Geometrie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required IReadOnlyCollection<WegknoopGeometrieProjectie> Geometrie { get; set; }

    /// <summary>
    /// GeoJSON-geometrietype.
    /// </summary>
    [DataMember(Name = "Type", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Type { get; } = "Point";
}

[DataContract(Name = "WegknoopGeometrieProjectie", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopGeometrieProjectie")]
public class WegknoopGeometrieProjectie
{
    [DataMember(Name = "gml", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Gml { get; set; }
}

public class WegknoopV2DetailResponseExamples : IExamplesProvider<WegknoopV2Detail>
{
    public WegknoopV2Detail GetExamples()
    {
        throw new NotImplementedException(); //TODO-pr implement example
        // return new GetRoadNodeResponseV2
        // {
        //     Identificator = new WegknoopIdentificatorV2("https://data.vlaanderen.be/id/wegknoop", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
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
        //     MethodeWegknoopgeometrie = RoadNodeGeometryDrawMethod.Outlined.ToDutchString(),
        //     BeginknoopObjectId = 287335,
        //     EindknoopObjectId = 287336,
        //     Linkerstraatnaam = new WegknoopStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Rechterstraatnaam = new WegknoopStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Wegknoopstatus = RoadNodeStatus.InUse.ToDutchString(),
        //     MorfologischeWegklasse = RoadNodeMorphology.Road_consisting_of_one_roadway.ToDutchString(),
        //     Toegangsbeperking = RoadNodeAccessRestriction.PublicRoad.ToDutchString(),
        //     Wegbeheerder = "1304",
        //     Wegcategorie = RoadNodeCategory.LocalRoadType3.ToDutchString(),
        //     Wegverharding = new[]
        //     {
        //         new WegverhardingObject
        //         {
        //             VanPositie = 0.000,
        //             TotPositie = 44.877,
        //             Verharding = RoadNodeSurfaceType.LooseSurface.ToDutchString()
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
        //             Richting = RoadNodeLaneDirection.Independent.ToDutchString()
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
        //             Richting = RoadNodeNumberedRoadDirection.Forward.ToDutchString(),
        //             Volgnummer = new RoadNodeNumberedRoadOrdinal(2686).ToDutchString()
        //         }
        //     }
        // };
    }
}
