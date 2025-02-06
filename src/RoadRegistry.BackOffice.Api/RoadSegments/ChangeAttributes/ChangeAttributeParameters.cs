namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System.Runtime.Serialization;
using Infrastructure;
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

    /// <summary>
    ///     De gekoppelde Europese wegen.
    /// </summary>
    [DataMember(Name = "EuropeseWegen", Order = 7)]
    [JsonProperty("europeseWegen")]
    public ChangeAttributeEuropeanRoad[] EuropeseWegen { get; set; }

    /// <summary>
    ///     De gekoppelde nationale wegen.
    /// </summary>
    [DataMember(Name = "NationaleWegen", Order = 8)]
    [JsonProperty("nationaleWegen")]
    public ChangeAttributeNationalRoad[] NationaleWegen { get; set; }

    /// <summary>
    ///     De gekoppelde genummerde wegen.
    /// </summary>
    [DataMember(Name = "GenummerdeWegen", Order = 9)]
    [JsonProperty("genummerdeWegen")]
    public ChangeAttributeNumberedRoad[] GenummerdeWegen { get; set; }

    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de linkerzijde van het wegsegment of "niet van toepassing".
    /// </summary>
    [DataMember(Name = "LinkerstraatnaamId", Order = 10)]
    [JsonProperty("linkerstraatnaamId")]
    public string? LinkerstraatnaamId { get; set; }

    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de rechterzijde van het wegsegment of "niet van toepassing".
    /// </summary>
    [DataMember(Name = "RechterstraatnaamId", Order = 11)]
    [JsonProperty("rechterstraatnaamId")]
    public string? RechterstraatnaamId { get; set; }
}

[DataContract(Name = "EuropeseWeg", Namespace = "")]
[CustomSwaggerSchemaId("EuropeseWeg")]
public class ChangeAttributeEuropeanRoad
{
    /// <summary>
    ///     Nummer van de Europese weg.
    /// </summary>
    [DataMember(Name = "EuNummer", Order = 1)]
    [JsonProperty("euNummer", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(EuropeanRoadNumber))]
    public string? EuNummer { get; set; }
}

[DataContract(Name = "NationaleWeg", Namespace = "")]
[CustomSwaggerSchemaId("NationaleWeg")]
public class ChangeAttributeNationalRoad
{
    /// <summary>
    ///     Ident2 van de nationale weg.
    /// </summary>
    [DataMember(Name = "Ident2", Order = 1)]
    [JsonProperty("Ident2", Required = Required.Always)]
    public string? Ident2 { get; set; }
}

[DataContract(Name = "GenummerdeWeg", Namespace = "")]
[CustomSwaggerSchemaId("GenummerdeWeg")]
public class ChangeAttributeNumberedRoad
{
    /// <summary>
    ///     Ident8 van de genummerde weg.
    /// </summary>
    [DataMember(Name = "Ident8", Order = 1)]
    [JsonProperty("ident8", Required = Required.Always)]
    public string? Ident8 { get; set; }

    /// <summary>
    ///     Richting van de genummerde weg.
    /// </summary>
    [DataMember(Name = "Richting", Order = 2)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentNumberedRoadDirection))]
    public string? Richting { get; set; }

    /// <summary>
    ///     Volgnummer van de genummerde weg (geheel positief getal of "niet gekend").
    /// </summary>
    [DataMember(Name = "Volgnummer", Order = 3)]
    [JsonProperty("volgnummer", Required = Required.Always)]
    public string? Volgnummer { get; set; }
}
