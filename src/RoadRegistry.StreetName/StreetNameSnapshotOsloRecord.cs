namespace RoadRegistry.StreetName;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
using Newtonsoft.Json;

public class StreetNameSnapshotOsloRecord
{
    [JsonProperty("@context")]
    public string Context { get; set; }
    [JsonProperty("@type")]
    public string Type { get; set; }
    [JsonProperty("identificator")]
    public DeseriazableIdentificator Identificator { get; set; }
    [JsonProperty("gemeente")]
    public StraatnaamDetailGemeente Gemeente { get; set; }
    [JsonProperty("straatnamen")]
    public List<DeseriazableGeografischeNaam> Straatnamen { get; set; }
    [JsonProperty("homoniemToevoegingen")]
    public List<DeseriazableGeografischeNaam> HomoniemToevoegingen { get; set; }
    [JsonProperty("straatnaamStatus")]
    public string StraatnaamStatus { get; set; }
}

public class DeseriazableIdentificator : Identificator
{
    public DeseriazableIdentificator()
        : base(string.Empty, string.Empty, string.Empty)
    {
    }

    public DeseriazableIdentificator(string naamruimte, string objectId, string versie)
        : base(naamruimte, objectId, versie)
    {
    }
}

public class DeseriazableGeografischeNaam : GeografischeNaam
{
    public DeseriazableGeografischeNaam()
        : base(string.Empty, default)
    {
    }

    public DeseriazableGeografischeNaam(string spelling, Taal taalCode)
        : base(spelling, taalCode)
    {
    }
}
