namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System.Runtime.Serialization;
using Newtonsoft.Json;

[DataContract(Name = "WegsegmentDynamischeAttributenWijzigen", Namespace = "")]
public record ChangeRoadSegmentDynamicAttributesParameters
{
    /// <summary>
    ///     Identificator van het wegsegment waarvoor de attributen moeten gewijzigd worden.
    /// </summary>
    [DataMember(Name = "WegsegmentId", Order = 1)]
    [JsonProperty("wegsegmentId", Required = Required.Always)]
    public int? WegsegmentId { get; set; }
    
    /// <summary>
    ///     Lineair gerefereerd attribuut dat het type wegverharding van een wegsegment aanduidt.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 2)]
    [JsonProperty("wegverharding")]
    public ChangeSurfaceAttributeParameters[] Wegverharding { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat de rijbaanbreedte van een wegsegment aanduidt (in meter).
    /// </summary>
    [DataMember(Name = "Wegbreedte", Order = 3)]
    [JsonProperty("wegbreedte")]
    public ChangeWidthAttributeParameters[] Wegbreedte { get; set; }

    /// <summary>
    ///     Lineair gerefereerd attribuut dat het aantal rijstroken van een wegsegment aanduidt.
    /// </summary>
    [DataMember(Name = "AantalRijstroken", Order = 4)]
    [JsonProperty("aantalRijstroken")]
    public ChangeLaneAttributeParameters[] AantalRijstroken { get; set; }
}
