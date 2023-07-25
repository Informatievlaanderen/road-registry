namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System.Runtime.Serialization;
using Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;

public record ChangeAttributeParameters
{
    /// <summary>
    ///     Lijst van identificatoren van wegsegmenten waarvoor de attributen moeten gewijzigd worden.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 1)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[] Wegsegmenten { get; set; }

    /// <summary>
    ///     De organisatie die verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 2)]
    [JsonProperty("wegbeheerder")]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "WegsegmentStatus", Order = 3)]
    [JsonProperty("wegsegmentstatus")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentStatus.Edit))]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     Beschrijft bepaalde aspecten van de morfologische vorm die een weg kan aannemen.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 4)]
    [JsonProperty("morfologischeWegklasse")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentMorphology.Edit))]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegankelijkheid van het wegsegment voor de weggebruiker.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 5)]
    [JsonProperty("toegangsbeperking")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentAccessRestriction))]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     Wegcategorie zoals gedefinieerd in het Ruimtelijke Structuurplan Vlaanderen.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 6)]
    [JsonProperty("wegcategorie")]
    [RoadRegistryEnumDataType(typeof(RoadSegmentCategory))]
    public string Wegcategorie { get; set; }
}
