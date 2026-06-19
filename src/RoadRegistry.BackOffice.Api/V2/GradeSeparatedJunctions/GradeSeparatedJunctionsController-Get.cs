namespace RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;

using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
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

public partial class GradeSeparatedJunctionsController
{
    private const string GetGradeSeparatedJunctionRoute = "{id}";

    /// <summary>
    ///     Vraag een ongelijkgrondse kruising op.
    /// </summary>
    /// <param name="id">De identificator van het ongelijkgrondse kruising.</param>
    /// <param name="responseOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het ongelijkgrondse kruising gevonden is.</response>
    /// <response code="404">Als het ongelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als het ongelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetGradeSeparatedJunctionRoute, Name = nameof(GetGradeSeparatedJunctionV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OngelijkgrondseKruisingV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(OngelijkgrondseKruisingV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeSeparatedJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeSeparatedJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetGradeSeparatedJunctionV2), Description = "Vraag een ongelijkgrondse kruising op.")]
    public async Task<IActionResult> GetGradeSeparatedJunctionV2(
        [FromRoute] int id,
        [FromServices] IOptions<ResponseOptions> responseOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var gradeSeparatedJunction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(id, cancellationToken);
        if (gradeSeparatedJunction is null)
        {
            return NotFound();
        }

        if (gradeSeparatedJunction.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new OngelijkgrondseKruisingV2Detail
        {
            Identificator = new Identificator(responseOptions.Value.OngelijkgrondseKruisingNaamruimte, gradeSeparatedJunction.GradeSeparatedJunctionId.ToString(), gradeSeparatedJunction.Origin.Timestamp.ToDateTimeOffset()),
            OnderliggendWegsegment = new WegsegmentLink(gradeSeparatedJunction.LowerRoadSegmentId, responseOptions.Value.WegsegmentDetailUrlFormat),
            BovenliggendWegsegment = new WegsegmentLink(gradeSeparatedJunction.UpperRoadSegmentId, responseOptions.Value.WegsegmentDetailUrlFormat)
        };

        //TODO-pr to confirm what to return with V1 data
        if (gradeSeparatedJunction.IsV2)
        {
            result.OngelijkgrondseKruisingType = GradeSeparatedJunctionTypeV2.Parse(gradeSeparatedJunction.Type!).ToDutchString();
        }

        return Ok(result);
    }
}

[DataContract(Name = "OngelijkgrondseKruisingV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("OngelijkgrondseKruisingV2Detail")]
public class OngelijkgrondseKruisingV2Detail
{
    /// <summary>
    ///     Identificerende gegevens van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required Identificator Identificator { get; set; }

    /// <summary>
    ///     Identificerende gegevens van het wegsegment dat zich onderaan de kruising bevindt.
    /// </summary>
    [DataMember(Name = "onderliggendWegsegment", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegsegmentLink OnderliggendWegsegment { get; set; }

    /// <summary>
    ///     Identificerende gegevens van het wegsegment dat zich bovenaan de kruising bevindt.
    /// </summary>
    [DataMember(Name = "bovenliggendWegsegment", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegsegmentLink BovenliggendWegsegment { get; set; }

    /// <summary>
    ///     Het type ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ongelijkgrondseKruisingType", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(GradeSeparatedJunctionTypeV2))]
    public string OngelijkgrondseKruisingType { get; set; }
}

public class OngelijkgrondseKruisingV2DetailResponseExamples : IExamplesProvider<OngelijkgrondseKruisingV2Detail>
{
    public OngelijkgrondseKruisingV2Detail GetExamples()
    {
        throw new NotImplementedException(); //TODO-pr implement example
        // return new GetGradeSeparatedJunctionResponseV2
        // {
        //     Identificator = new Ongelijkgrondse KruisingIdentificatorV2("https://data.vlaanderen.be/id/ongelijkgrondse kruising", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
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
        //     MethodeOngelijkgrondse Kruisinggeometrie = GradeSeparatedJunctionGeometryDrawMethod.Outlined.ToDutchString(),
        //     BeginknoopObjectId = 287335,
        //     EindknoopObjectId = 287336,
        //     Linkerstraatnaam = new Ongelijkgrondse KruisingStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Rechterstraatnaam = new Ongelijkgrondse KruisingStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Ongelijkgrondse Kruisingstatus = GradeSeparatedJunctionStatus.InUse.ToDutchString(),
        //     MorfologischeWegklasse = GradeSeparatedJunctionMorphology.Road_consisting_of_one_roadway.ToDutchString(),
        //     Toegangsbeperking = GradeSeparatedJunctionAccessRestriction.PublicRoad.ToDutchString(),
        //     Wegbeheerder = "1304",
        //     Wegcategorie = GradeSeparatedJunctionCategory.LocalRoadType3.ToDutchString(),
        //     Wegverharding = new[]
        //     {
        //         new WegverhardingObject
        //         {
        //             VanPositie = 0.000,
        //             TotPositie = 44.877,
        //             Verharding = GradeSeparatedJunctionSurfaceType.LooseSurface.ToDutchString()
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
        //             Richting = GradeSeparatedJunctionLaneDirection.Independent.ToDutchString()
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
        //             Richting = GradeSeparatedJunctionNumberedRoadDirection.Forward.ToDutchString(),
        //             Volgnummer = new GradeSeparatedJunctionNumberedRoadOrdinal(2686).ToDutchString()
        //         }
        //     }
        // };
    }
}
