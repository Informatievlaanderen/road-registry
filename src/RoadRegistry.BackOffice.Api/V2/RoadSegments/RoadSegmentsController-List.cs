namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
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

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Vraag een lijst met wegsegmenten op.
    /// </summary>
    /// <param name="apiOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de opvraging van een lijst met wegsegmenten gelukt is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(Name = nameof(ListRoadSegmentsV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WegsegmentV2Lijst), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(WegsegmentV2LijstResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(ListRoadSegmentsV2), Description = "Vraag een lijst met wegsegmenten op.")]
    public async Task<IActionResult> ListRoadSegmentsV2(
        [FromServices] ApiOptions apiOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        // Filtering and paging come in the headers the public API sets, the way the other registers' list endpoints
        // take them.
        var filter = Request.ExtractFilteringRequest<RoadSegmentFilter>().Filter ?? new RoadSegmentFilter();
        var pagination = Request.ExtractPaginationRequest() as PaginationRequest ?? new PaginationRequest(0, 100);
        var paginationInfo = new PaginationInfo(pagination.Offset, pagination.Limit, pagination.Limit > 0);

        var roadSegments = await ListRoadSegments(store, filter, pagination, cancellationToken);

        var wegsegmenten = roadSegments
            .Select(x => new WegsegmentV2LijstItem(x.RoadSegmentId, x.GetStatusV2(), apiOptions.GetWegsegmentDetailUrlFormat()))
            .ToList();

        Response.AddPaginationResponse(paginationInfo);

        return Ok(new WegsegmentV2Lijst
        {
            Wegsegmenten = wegsegmenten,
            Volgende = paginationInfo.HasNextPage(wegsegmenten.Count)
                ? new Uri(string.Format(apiOptions.GetWegsegmentenVolgendeUrlFormat(), pagination.Offset + pagination.Limit, pagination.Limit))
                : null
        });
    }

    // A filter value that cannot match anything - a street name id that is not a number, a status that does not exist -
    // matches nothing, the way it does in the other registers' lists: an empty list rather than a bad request.
    private static async Task<IReadOnlyList<RoadSegmentReadItem>> ListRoadSegments(
        IDocumentStore store,
        RoadSegmentFilter filter,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        if (pagination.Limit == 0)
        {
            return [];
        }

        StreetNameLocalId? streetNameId = null;
        if (!string.IsNullOrEmpty(filter.StreetNameId))
        {
            if (!int.TryParse(filter.StreetNameId, out var parsedStreetNameId) || parsedStreetNameId <= 0)
            {
                return [];
            }

            streetNameId = new StreetNameLocalId(parsedStreetNameId);
        }

        RoadSegmentStatusV2? status = null;
        if (!string.IsNullOrEmpty(filter.Status))
        {
            if (!RoadSegmentStatusV2.TryParseUsingDutchName(filter.Status, out var parsedStatus))
            {
                return [];
            }

            status = parsedStatus;
        }

        await using var session = store.LightweightSession();

        return await session.ListRoadSegments(streetNameId, status, pagination.Offset, pagination.Limit, cancellationToken);
    }
}

public class RoadSegmentFilter
{
    public string? StreetNameId { get; set; }
    public string? Status { get; set; }
}

/// <summary>
///     Een verzameling van wegsegmenten.
/// </summary>
[DataContract(Name = "WegsegmentV2Lijst", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2Lijst")]
public class WegsegmentV2Lijst
{
    /// <summary>
    ///     De verzameling van wegsegmenten.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required List<WegsegmentV2LijstItem> Wegsegmenten { get; set; }

    /// <summary>
    ///     De URL voor het ophalen van de volgende verzameling.
    /// </summary>
    [DataMember(Name = "Volgende", Order = 2, EmitDefaultValue = false)]
    [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Uri? Volgende { get; set; }
}

/// <summary>
///     Een wegsegment in een verzameling van wegsegmenten.
/// </summary>
[DataContract(Name = "WegsegmentV2LijstItem", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentV2LijstItem")]
public class WegsegmentV2LijstItem
{
    /// <summary>
    ///     De objectidentificator van het wegsegment.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string ObjectId { get; set; }

    /// <summary>
    ///     Attribuut dat de levensloopfase van een wegsegment aangeeft.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatusV2))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     Link naar het detail van het wegsegment.
    /// </summary>
    [DataMember(Name = "Detail", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Detail { get; set; }

    public WegsegmentV2LijstItem(RoadSegmentId roadSegmentId, RoadSegmentStatusV2 status, string detailUrlFormat)
    {
        ObjectId = roadSegmentId.ToString();
        Wegsegmentstatus = status.ToDutchString();
        Detail = string.Format(detailUrlFormat, roadSegmentId);
    }
}

public class WegsegmentV2LijstResponseExamples : IExamplesProvider<WegsegmentV2Lijst>
{
    private readonly ApiOptions _apiOptions;

    public WegsegmentV2LijstResponseExamples(ApiOptions apiOptions)
    {
        _apiOptions = apiOptions;
    }

    public WegsegmentV2Lijst GetExamples()
    {
        return new WegsegmentV2Lijst
        {
            Wegsegmenten =
            [
                new WegsegmentV2LijstItem(new RoadSegmentId(123456), RoadSegmentStatusV2.Gerealiseerd, _apiOptions.GetWegsegmentDetailUrlFormat()),
                new WegsegmentV2LijstItem(new RoadSegmentId(123457), RoadSegmentStatusV2.Gepland, _apiOptions.GetWegsegmentDetailUrlFormat())
            ],
            Volgende = new Uri(string.Format(_apiOptions.GetWegsegmentenVolgendeUrlFormat(), 2, 2))
        };
    }
}
