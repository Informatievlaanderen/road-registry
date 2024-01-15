namespace RoadRegistry.Sync.StreetNameRegistry;

public class StreetNameRecord
{
    public int PersistentLocalId { get; set; }
    public string StreetNameId { get; set; }
    public string NisCode { get; set; }
    public string DutchName { get; set; }
    public string FrenchName { get; set; }
    public string GermanName { get; set; }
    public string EnglishName { get; set; }
    public string DutchHomonymAddition { get; set; }
    public string FrenchHomonymAddition { get; set; }
    public string GermanHomonymAddition { get; set; }
    public string EnglishHomonymAddition { get; set; }
    public string DutchNameWithHomonymAddition { get; set; }
    public string FrenchNameWithHomonymAddition { get; set; }
    public string GermanNameWithHomonymAddition { get; set; }
    public string EnglishNameWithHomonymAddition { get; set; }
    public string StreetNameStatus { get; set; }
    public bool IsRemoved { get; set; }
}
