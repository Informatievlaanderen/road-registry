namespace RoadRegistry.BackOffice.Api.V2.GradeJunctions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="id">De identificator van de gelijkgrondse kruising.</param>
    /// <param name="apiOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de gelijkgrondse kruising gevonden is.</response>
    /// <response code="404">Als de gelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als de gelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetGradeJunctionRoute, Name = nameof(GetGradeJunctionV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GelijkgrondseKruisingV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GelijkgrondseKruisingV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetGradeJunctionV2), Description = "Vraag een gelijkgrondse kruising op.")]
    public async Task<IActionResult> GetGradeJunctionV2(
        [FromRoute] int id,
        [FromServices] ApiOptions apiOptions,
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

        var result = new GelijkgrondseKruisingV2Detail
        {
            Identificator = new GelijkgrondseKruisingIdentificator(OsloNamespaces.GelijkgrondseKruising, gradeJunction.GradeJunctionId.ToString(), gradeJunction.Origin.Timestamp.ToDateTimeOffset()),
            KruisendeWegsegmenten = new[] { gradeJunction.RoadSegmentId1, gradeJunction.RoadSegmentId2 }
                .OrderBy(x => x)
                .Select(x => new WegsegmentLink(x, apiOptions.GetWegsegmentDetailUrlFormat()))
                .ToArray()
        };

        return Ok(result);
    }
}

[DataContract(Name = "GelijkgrondseKruisingV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("GelijkgrondseKruisingV2Detail")]
public class GelijkgrondseKruisingV2Detail
{
    /// <summary>
    ///     Identificerende gegevens van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required GelijkgrondseKruisingIdentificator Identificator { get; set; }

    /// <summary>
    ///     Identificerende gegevens van de twee wegsegmenten die elkaar gelijkgronds kruisen.
    /// </summary>
    [DataMember(Name = "kruisendeWegsegmenten", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public WegsegmentLink[] KruisendeWegsegmenten { get; set; }
}

[DataContract(Name = "GelijkgrondseKruisingV2Identificator", Namespace = "")]
[CustomSwaggerSchemaId("GelijkgrondseKruisingV2Identificator")]
public class GelijkgrondseKruisingIdentificator : Identificator
{
    public GelijkgrondseKruisingIdentificator(string naamruimte, string objectId, DateTimeOffset? versie)
        : base(naamruimte, objectId, versie)
    {
    }

    public GelijkgrondseKruisingIdentificator()
        : base(string.Empty, string.Empty, string.Empty)
    {
    }
}

public class GelijkgrondseKruisingV2DetailResponseExamples : IExamplesProvider<GelijkgrondseKruisingV2Detail>
{
    private readonly ApiOptions _apiOptions;

    public GelijkgrondseKruisingV2DetailResponseExamples(ApiOptions apiOptions)
    {
        _apiOptions = apiOptions;
    }

    public GelijkgrondseKruisingV2Detail GetExamples()
    {
        return new GelijkgrondseKruisingV2Detail
        {
            Identificator = new GelijkgrondseKruisingIdentificator(OsloNamespaces.GelijkgrondseKruising, "643556", new DateTimeOffset(2026, 09, 27, 13, 46, 14, TimeSpan.FromHours(2))),
            KruisendeWegsegmenten =
            [
                new WegsegmentLink(new RoadSegmentId(51613), _apiOptions.GetWegsegmentDetailUrlFormat()),
                new WegsegmentLink(new RoadSegmentId(89134), _apiOptions.GetWegsegmentDetailUrlFormat())
            ]
        };
    }
}
