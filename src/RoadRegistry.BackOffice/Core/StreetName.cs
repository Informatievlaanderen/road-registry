namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class StreetName : EventSourcedEntity
{
    public static readonly Func<StreetName> Factory = () => new StreetName();

    private StreetName()
    {
        On<StreetNameCreated>(e =>
        {
            ReadFromRecord(e.Record);
        });
        On<StreetNameModified>(e =>
        {
            ReadFromRecord(e.Record);
            IsRemoved = false;
        });
        On<StreetNameRemoved>(e =>
        {
            IsRemoved = true;
        });
    }

    public StreetNameId StreetNameId { get; private set; }
    public StreetNameLocalId PersistentLocalId { get; private set; }
    public string NisCode { get; private set; }
    public string DutchName { get; private set; }
    public string FrenchName { get; private set; }
    public string GermanName { get; private set; }
    public string EnglishName { get; private set; }
    public string DutchHomonymAddition { get; private set; }
    public string FrenchHomonymAddition { get; private set; }
    public string GermanHomonymAddition { get; private set; }
    public string EnglishHomonymAddition { get; private set; }
    public string DutchNameWithHomonymAddition { get; private set; }
    public string FrenchNameWithHomonymAddition { get; private set; }
    public string GermanNameWithHomonymAddition { get; private set; }
    public string EnglishNameWithHomonymAddition { get; private set; }
    public string StreetNameStatus { get; private set; }
    public bool IsRemoved { get; private set; }

    private void ReadFromRecord(StreetNameRecord record)
    {
        StreetNameId = new StreetNameId(record.StreetNameId);
        PersistentLocalId = new StreetNameLocalId(record.PersistentLocalId);
        NisCode = record.NisCode;
        DutchName = record.DutchName;
        FrenchName = record.FrenchName;
        GermanName = record.GermanName;
        EnglishName = record.EnglishName;
        DutchHomonymAddition = record.DutchHomonymAddition;
        FrenchHomonymAddition = record.FrenchHomonymAddition;
        GermanHomonymAddition = record.GermanHomonymAddition;
        EnglishHomonymAddition = record.EnglishHomonymAddition;
        DutchNameWithHomonymAddition = record.DutchNameWithHomonymAddition;
        FrenchNameWithHomonymAddition = record.FrenchNameWithHomonymAddition;
        GermanNameWithHomonymAddition = record.GermanNameWithHomonymAddition;
        EnglishNameWithHomonymAddition = record.EnglishNameWithHomonymAddition;
        StreetNameStatus = record.StreetNameStatus;
    }
}
