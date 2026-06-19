namespace RoadRegistry.BackOffice.Api.V2.GradeJunctions;

using System;
using System.Collections.Generic;
using System.Linq;
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
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.Read.Projections;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class GradeJunctionsController
{
    private const string GetGradeJunctionRoute = "{id}";

    /// <summary>
    ///     Vraag een gelijkgrondse kruising op.
    /// </summary>
    /// <param name="id">De identificator van het gelijkgrondse kruising.</param>
    /// <param name="responseOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het gelijkgrondse kruising gevonden is.</response>
    /// <response code="404">Als het gelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als het gelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetGradeJunctionRoute, Name = nameof(GetGradeJunctionV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GelijkgrondsekruisingV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GelijkgrondsekruisingV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetGradeJunctionV2), Description = "Vraag een gelijkgrondse kruising op.")]
    public async Task<IActionResult> GetGradeJunctionV2(
        [FromRoute] int id,
        [FromServices] IOptions<ResponseOptions> responseOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var gradeJunction = await session.LoadAsync<GradeJunctionReadItem>(id, cancellationToken);
        if (gradeJunction is null)
        {
            return NotFound();
        }

        if (gradeJunction.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new GelijkgrondsekruisingV2Detail
        {
            Identificator = new Identificator(responseOptions.Value.GelijkgrondseKruisingNaamruimte, gradeJunction.GradeJunctionId.ToString(), gradeJunction.Origin.Timestamp.ToDateTimeOffset()),
            KruisendeWegsegmenten = new [] { gradeJunction.RoadSegmentId1, gradeJunction.RoadSegmentId2 }
                .OrderBy(x => x)
                .Select(x => new WegsegmentLink(x, responseOptions.Value.WegsegmentDetailUrlFormat))
                .ToArray()
        };

        return Ok(result);
    }
}

[DataContract(Name = "GelijkgrondsekruisingV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("GelijkgrondsekruisingV2Detail")]
public class GelijkgrondsekruisingV2Detail
{
    /// <summary>
    ///     Identificerende gegevens van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required Identificator Identificator { get; set; }

    /// <summary>
    ///     Identificerende gegevens van de twee wegsegmenten die elkaar gelijkgronds kruisen.
    /// </summary>
    [DataMember(Name = "kruisendeWegsegmenten", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public IReadOnlyCollection<WegsegmentLink> KruisendeWegsegmenten { get; set; }
}

public class GelijkgrondsekruisingV2DetailResponseExamples : IExamplesProvider<GelijkgrondsekruisingV2Detail>
{
    public GelijkgrondsekruisingV2Detail GetExamples()
    {
        throw new NotImplementedException(); //TODO-pr implement example
        // return new GetGradeJunctionResponseV2
        // {
        //     Identificator = new Gelijkgrondse KruisingIdentificatorV2("https://data.vlaanderen.be/id/gelijkgrondse kruising", "643556", new DateTime(2015, 11, 27, 13, 46, 14), 2),
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
        //     MethodeGelijkgrondse Kruisinggeometrie = GradeJunctionGeometryDrawMethod.Outlined.ToDutchString(),
        //     BeginknoopObjectId = 287335,
        //     EindknoopObjectId = 287336,
        //     Linkerstraatnaam = new Gelijkgrondse KruisingStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Rechterstraatnaam = new Gelijkgrondse KruisingStraatnaamObject
        //     {
        //         ObjectId = new StreetNameId(71671).ToString(),
        //         Straatnaam = "Smidsestraat"
        //     },
        //     Gelijkgrondse Kruisingstatus = GradeJunctionStatus.InUse.ToDutchString(),
        //     MorfologischeWegklasse = GradeJunctionMorphology.Road_consisting_of_one_roadway.ToDutchString(),
        //     Toegangsbeperking = GradeJunctionAccessRestriction.PublicRoad.ToDutchString(),
        //     Wegbeheerder = "1304",
        //     Wegcategorie = GradeJunctionCategory.LocalRoadType3.ToDutchString(),
        //     Wegverharding = new[]
        //     {
        //         new WegverhardingObject
        //         {
        //             VanPositie = 0.000,
        //             TotPositie = 44.877,
        //             Verharding = GradeJunctionSurfaceType.LooseSurface.ToDutchString()
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
        //             Richting = GradeJunctionLaneDirection.Independent.ToDutchString()
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
        //             Richting = GradeJunctionNumberedRoadDirection.Forward.ToDutchString(),
        //             Volgnummer = new GradeJunctionNumberedRoadOrdinal(2686).ToDutchString()
        //         }
        //     }
        // };
    }
}
