namespace RoadRegistry.Syndication.Schema;

using System;

public class StreetNameRecord
{
    public string DutchHomonymAddition { get; set; }
    public string DutchName { get; set; }
    public string DutchNameWithHomonymAddition { get; set; }
    public string EnglishHomonymAddition { get; set; }
    public string EnglishName { get; set; }
    public string EnglishNameWithHomonymAddition { get; set; }
    public string FrenchHomonymAddition { get; set; }
    public string FrenchName { get; set; }
    public string FrenchNameWithHomonymAddition { get; set; }
    public string GermanHomonymAddition { get; set; }
    public string GermanName { get; set; }
    public string GermanNameWithHomonymAddition { get; set; }

    public string HomonymAddition { get; set; }
    public Guid MunicipalityId { get; set; }
    public string Name { get; set; }

    public string NameWithHomonymAddition { get; set; }
    public string NisCode { get; set; }
    public int? PersistentLocalId { get; set; }
    public long Position { get; set; }
    public Guid StreetNameId { get; set; }

    public StreetNameStatus? StreetNameStatus { get; set; }
}
