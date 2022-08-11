namespace RoadRegistry.Wms.Projections.Framework;

using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class NullToInt32Converter : NullableConverter
{
    public NullToInt32Converter(Type type, TypeConverterCache typeConverterFactory) : base(type, typeConverterFactory)
    {
    }

    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == "NULL" && UnderlyingType == typeof(int))
            return null;
        return base.ConvertFromString(text, row, memberMapData);
    }
}

public class NullToStringConverter : StringConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == "NULL")
            return null;
        return base.ConvertFromString(text, row, memberMapData);
    }
}
