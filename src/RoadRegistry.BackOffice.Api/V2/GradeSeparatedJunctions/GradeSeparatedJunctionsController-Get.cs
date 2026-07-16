namespace RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;

using System;
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
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.Read.Projections;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Identificator = Be.Vlaanderen.Basisregisters.GrAr.Legacy.Identificator;

public partial class GradeSeparatedJunctionsController
{
    private const string GetGradeSeparatedJunctionRoute = "{id}";

    /// <summary>
    ///     Vraag een ongelijkgrondse kruising op.
    /// </summary>
    /// <param name="id">De identificator van de ongelijkgrondse kruising.</param>
    /// <param name="apiOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de ongelijkgrondse kruising gevonden is.</response>
    /// <response code="404">Als de ongelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als de ongelijkgrondse kruising is verwijderd.</response>
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
        [FromServices] ApiOptions apiOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var gradeSeparatedJunction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(id, cancellationToken);
        if (gradeSeparatedJunction is null || !gradeSeparatedJunction.IsV2)
        {
            return NotFound();
        }

        if (gradeSeparatedJunction.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new OngelijkgrondseKruisingV2Detail
        {
            Identificator = new OngelijkgrondseKruisingIdentificator(OsloNamespaces.OngelijkgrondseKruising, gradeSeparatedJunction.GradeSeparatedJunctionId.ToString(), gradeSeparatedJunction.Origin.Timestamp.ToDateTimeOffset()),
            OnderliggendWegsegment = new WegsegmentLink(gradeSeparatedJunction.LowerRoadSegmentId, apiOptions.GetWegsegmentDetailUrlFormat()),
            BovenliggendWegsegment = new WegsegmentLink(gradeSeparatedJunction.UpperRoadSegmentId, apiOptions.GetWegsegmentDetailUrlFormat()),
            OngelijkgrondseKruisingType = GradeSeparatedJunctionTypeV2.Parse(gradeSeparatedJunction.Type!).ToDutchString()
        };

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
    public required OngelijkgrondseKruisingIdentificator Identificator { get; set; }

    /// <summary>
    ///     Identificerende gegevens van het wegsegment dat zich onderaan de kruising bevindt.
    /// </summary>
    [DataMember(Name = "onderliggendWegsegment", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegsegmentLink OnderliggendWegsegment { get; set; }

    /// <summary>
    ///     Identificerende gegevens van het wegsegment dat zich bovenaan de kruising bevindt.
    /// </summary>
    [DataMember(Name = "bovenliggendWegsegment", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegsegmentLink BovenliggendWegsegment { get; set; }

    /// <summary>
    ///     Het type ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ongelijkgrondseKruisingType", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(GradeSeparatedJunctionTypeV2))]
    public required string OngelijkgrondseKruisingType { get; set; }
}

[DataContract(Name = "OngelijkgrondseKruisingV2Identificator", Namespace = "")]
[CustomSwaggerSchemaId("OngelijkgrondseKruisingV2Identificator")]
public class OngelijkgrondseKruisingIdentificator : Identificator
{
    public OngelijkgrondseKruisingIdentificator(string naamruimte, string objectId, DateTimeOffset? versie)
        : base(naamruimte, objectId, versie)
    {
    }

    public OngelijkgrondseKruisingIdentificator()
        : base(string.Empty, string.Empty, string.Empty)
    {
    }
}

public class OngelijkgrondseKruisingV2DetailResponseExamples : IExamplesProvider<OngelijkgrondseKruisingV2Detail>
{
    private readonly ApiOptions _apiOptions;

    public OngelijkgrondseKruisingV2DetailResponseExamples(ApiOptions apiOptions)
    {
        _apiOptions = apiOptions;
    }

    public OngelijkgrondseKruisingV2Detail GetExamples()
    {
        return new OngelijkgrondseKruisingV2Detail
        {
            Identificator = new OngelijkgrondseKruisingIdentificator(OsloNamespaces.OngelijkgrondseKruising, "643556", new DateTimeOffset(2026, 09, 27, 13, 46, 14, TimeSpan.FromHours(2))),
            OnderliggendWegsegment = new WegsegmentLink(new RoadSegmentId(51613), _apiOptions.GetWegsegmentDetailUrlFormat()),
            BovenliggendWegsegment = new WegsegmentLink(new RoadSegmentId(89134), _apiOptions.GetWegsegmentDetailUrlFormat()),
            OngelijkgrondseKruisingType = GradeSeparatedJunctionTypeV2.Brug.ToDutchString()
        };
    }
}
