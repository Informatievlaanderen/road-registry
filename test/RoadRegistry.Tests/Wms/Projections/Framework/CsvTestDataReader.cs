namespace RoadRegistry.Wms.Projections.Framework;

using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

public class CsvTestDataReader : CsvReader
{
    public CsvTestDataReader(TextReader reader) :
        base(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            LeaveOpen = false
        })
    {
        base.Context.TypeConverterCache.AddConverter<int>(new NullToInt32Converter(typeof(int?), base.Context.TypeConverterCache));
        base.Context.TypeConverterCache.AddConverter<string>(new NullToStringConverter());
    }
}
