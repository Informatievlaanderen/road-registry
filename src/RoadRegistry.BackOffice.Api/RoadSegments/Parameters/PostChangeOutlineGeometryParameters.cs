namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Runtime.Serialization;
using Newtonsoft.Json;

[DataContract(Name = "WegsegmentSchetsGeometrieWijzigen", Namespace = "")]
public record PostChangeOutlineGeometryParameters
{
    /// <summary>
    ///     De geometrie die de middellijn van het wegsegment vertegenwoordigt, het formaat gml 3.2 (linestring) en
    ///     coördinatenstelsel Lambert 72 (EPSG:31370).
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 1)]
    [JsonProperty]
    public string MiddellijnGeometrie { get; set; }
}
