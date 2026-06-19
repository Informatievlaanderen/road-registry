namespace RoadRegistry.BackOffice.Api.V2;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;

[DataContract(Name = "WegsegmentLink", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentLink")]
public sealed class WegsegmentLink
{
    /// <summary>
    /// De objectidentificator van het wegsegment.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van het wegsegment.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public string Detail { get; set; }

    private WegsegmentLink()
    {
    }

    public WegsegmentLink(RoadSegmentId roadSegmentId, string detailUrlFormat)
    {
        ObjectId = roadSegmentId.ToString();
        Detail = string.Format(detailUrlFormat, roadSegmentId);
    }
}
