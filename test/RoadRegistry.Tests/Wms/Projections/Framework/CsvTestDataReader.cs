namespace RoadRegistry.Wms.Projections.Framework
{
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;

    public class CsvTestDataReader : CsvReader
    {
        public CsvTestDataReader(TextReader reader) :
            base(reader, CreateConfiguration(CultureInfo.InvariantCulture), false)
        {
        }

        private static CsvConfiguration CreateConfiguration(CultureInfo culture)
        {
            var csvConfiguration = new CsvConfiguration(culture)
            {
                CultureInfo = culture,
                HasHeaderRecord = true
            };

            csvConfiguration.TypeConverterCache.AddConverter<int>(new NullToInt32Converter(typeof(int?), csvConfiguration.TypeConverterCache));
            csvConfiguration.TypeConverterCache.AddConverter<string>(new NullToStringConverter());

            return csvConfiguration;
        }
    }
}
