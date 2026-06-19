namespace RoadRegistry.BackOffice.Api.V2;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;

[DataContract(Name = "GelijkgrondseKruisingLink", Namespace = "")]
[CustomSwaggerSchemaId("GelijkgrondseKruisingLink")]
public class GelijkgrondseKruisingLink
{
    /// <summary>
    /// De objectidentificator van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de gelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public string Detail { get; set; }

    public GelijkgrondseKruisingLink(GradeJunctionId gradeJunctionId, string detailUrlFormat)
    {
        ObjectId = gradeJunctionId.ToString();
        Detail = string.Format(detailUrlFormat, gradeJunctionId);
    }
}
