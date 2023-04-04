namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System.Runtime.Serialization;
using Newtonsoft.Json;

[DataContract(Name = "WegsegmentSchetsen", Namespace = "")]
public record PostRoadSegmentOutlineParameters
{
    /// <summary>
    ///     De geometrie die de middellijn van het wegsegment vertegenwoordigt, het formaat gml 3.2 (linestring) en
    ///     coördinatenstelsel Lambert 72 (EPSG:31370).
    /// </summary>
    [DataMember(Name = "MiddellijnGeometrie", Order = 1)]
    [JsonProperty]
    public string MiddellijnGeometrie { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegsegmentstatus", Order = 2)]
    [JsonProperty]
    public string Wegsegmentstatus { get; set; }

    /// <summary>
    ///     De wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 3)]
    [JsonProperty]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegankelijkheid van het wegsegment voor de weggebruiker.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 4)]
    [JsonProperty]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De organisatie die verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 5)]
    [JsonProperty]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     Type wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegverharding", Order = 6)]
    [JsonProperty]
    public string Wegverharding { get; set; }

    /// <summary>
    ///     Breedte van het wegsegment(in meter).
    /// </summary>
    [DataMember(Name = "Wegbreedte", Order = 7)]
    [JsonProperty]
    public int? Wegbreedte { get; set; }

    /// <summary>
    ///     Aantal rijstroken van het wegsegment, en hun richting t.o.v. de richting van het wegsegment (begin- naar
    ///     eindknoop).
    /// </summary>
    [DataMember(Name = "AantalRijstroken", Order = 8)]
    [JsonProperty]
    public RoadSegmentLaneParameters AantalRijstroken { get; set; }
}
