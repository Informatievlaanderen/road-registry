namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Runtime.Serialization;
using Newtonsoft.Json;

public class RoadSegmentLaneParameters
{
    /// <summary>
    ///     Aantal rijstroken van de wegsegmentschets.
    /// </summary>
    [DataMember(Name = "Aantal", Order = 1)]
    [JsonProperty]
    public int? Aantal { get; set; }

    /// <summary>
    ///     De richting van de wegsegmentschets (begin- naar eindknoop).
    /// </summary>
    [DataMember(Name = "Richting", Order = 2)]
    [JsonProperty]
    public string Richting { get; set; }
}
