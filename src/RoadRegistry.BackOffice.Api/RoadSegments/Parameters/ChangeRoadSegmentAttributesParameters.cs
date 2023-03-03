namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Runtime.Serialization;
using Newtonsoft.Json;

[DataContract(Name = "WegsegmentSchetsen", Namespace = "")]
public record ChangeRoadSegmentAttributesParameters
{
    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 1)]
    [JsonProperty]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     De wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 2)]
    [JsonProperty]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegankelijkheid van het wegsegment voor de weggebruiker.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 3)]
    [JsonProperty]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De organisatie die verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 4)]
    [JsonProperty]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     De wegcategorie van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 5)]
    [JsonProperty]
    public string Wegcategorie { get; set; }
}
