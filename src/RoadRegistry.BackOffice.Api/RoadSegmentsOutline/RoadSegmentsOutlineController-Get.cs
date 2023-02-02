namespace RoadRegistry.BackOffice.Api.RoadSegmentsOutline;

using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadSegments;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsOutlineController
{
    /// <summary>
    /// Vraag een wegsegment op.
    /// </summary>
    /// <param name="id">De identificator van het wegsegment.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetRoadSegmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetRoadSegmentResponseResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    public async Task<IActionResult> Get(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var detailResponse = await _mediator.Send(new RoadSegmentDetailRequest(id), cancellationToken);

            var result = new GetRoadSegmentResponse
            {
                Identificator = new Identificator
                {
                    ObjectId = detailResponse.RoadSegmentId.ToString(),
                    VersieId = detailResponse.BeginTime
                },
                Linkerstraatnaam = detailResponse.LeftStreetNameId != null ? new StraatnaamObject
                {
                    ObjectId = detailResponse.LeftStreetNameId.ToString(),
                    Straatnaam = detailResponse.LeftStreetName
                } : null,
                Rechterstraatnaam = detailResponse.RightStreetNameId != null ? new StraatnaamObject
                {
                    ObjectId = detailResponse.RightStreetNameId.ToString(),
                    Straatnaam = detailResponse.RightStreetName
                } : null
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

[DataContract(Name = "WegsegmentDetail", Namespace = "")]
public class GetRoadSegmentResponse
{
    /// <summary>
    /// De unieke identificator van het wegsegment.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty]
    public Identificator Identificator { get; set; }

    /// <summary>
    /// De identificator van de linkerstraatnaam.
    /// </summary>
    [DataMember(Name = "Linkerstraatnaam", Order = 2)]
    [JsonProperty]
    public StraatnaamObject Linkerstraatnaam { get; set; }

    /// <summary>
    /// De identificator van de rechterstraatnaam.
    /// </summary>
    [DataMember(Name = "Rechterstraatnaam", Order = 3)]
    [JsonProperty]
    public StraatnaamObject Rechterstraatnaam { get; set; }
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
            Linkerstraatnaam = new StraatnaamObject
            {
                ObjectId = "71671",
                Straatnaam = "Smidsestraat"
            },
            Rechterstraatnaam = new StraatnaamObject
            {
                ObjectId = "71671",
                Straatnaam = "Smidsestraat"
            }
        };
    }
}
