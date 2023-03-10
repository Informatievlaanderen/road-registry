namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Runtime.Serialization;
using Newtonsoft.Json;

public record ChangeAttributeParameters
{
    /// <summary>
    ///     Het attribuut die gewijzigd moet worden.
    /// </summary>
    [DataMember(Name = "attribuut", Order = 1)]
    [JsonProperty("attribuut")]
    public string Attribuut { get; set; }

    /// <summary>
    ///     De attribuutwaarde van het te wijzigen attribuut.
    /// </summary>
    [DataMember(Name = "attribuutwaarde", Order = 2)]
    [JsonProperty("attribuutwaarde")]
    public string Attribuutwaarde { get; set; }

    /// <summary>
    ///     Lijst van identificatoren van wegsegmenten waarvoor het attribuut moet gewijzigd worden.
    /// </summary>
    [DataMember(Name = "wegsegmenten", Order = 3)]
    [JsonProperty("wegsegmenten")]
    public int[] Wegsegmenten { get; set; }
}
