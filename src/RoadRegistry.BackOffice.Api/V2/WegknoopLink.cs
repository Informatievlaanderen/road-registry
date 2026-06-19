namespace RoadRegistry.BackOffice.Api.V2;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;

[DataContract(Name = "WegknoopLink", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopLink")]
public sealed class WegknoopLink
{
    /// <summary>
    /// De objectidentificator van de wegknoop.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de wegknoop.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public string Detail { get; set; }

    private WegknoopLink()
    {
    }

    public WegknoopLink(RoadNodeId roadNodeId, string detailUrlFormat)
    {
        ObjectId = roadNodeId.ToString();
        Detail = string.Format(detailUrlFormat, roadNodeId);
    }
}
