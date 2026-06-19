namespace RoadRegistry.BackOffice.Api.V2;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;

[DataContract(Name = "OngelijkgrondseKruisingLink", Namespace = "")]
[CustomSwaggerSchemaId("OngelijkgrondseKruisingLink")]
public class OngelijkgrondseKruisingLink
{
    /// <summary>
    /// De objectidentificator van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "ObjectId", Order = 1)]
    [JsonProperty]
    public string ObjectId { get; set; }

    /// <summary>
    /// Link naar het detail van de ongelijkgrondse kruising.
    /// </summary>
    [DataMember(Name = "Detail", Order = 2)]
    [JsonProperty]
    public string Detail { get; set; }

    public OngelijkgrondseKruisingLink(GradeSeparatedJunctionId gradeSeparatedJunctionId, string detailUrlFormat)
    {
        ObjectId = gradeSeparatedJunctionId.ToString();
        Detail = string.Format(detailUrlFormat, gradeSeparatedJunctionId);
    }
}
