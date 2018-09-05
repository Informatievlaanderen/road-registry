namespace RoadRegistry
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    public abstract class EnumTranslator<TEnum>
        where TEnum : IConvertible, IComparable, IFormattable
    {
        protected EnumTranslator()
        {
            // This would be some much simpler code if "where T : Enum" is supported
            if (!typeof(TEnum).IsEnum)
                throw new InvalidEnumArgumentException("TEnum must be an Enum");
        }

        protected abstract IDictionary<TEnum, string> DutchTranslations { get; }
        protected abstract IDictionary<TEnum, string> DutchDescriptions { get; }

        public int TranslateToIdentifier(TEnum value)
        {
            // This would be some much simpler code when "where T : Enum" is supported
            // supported from C# 7.3
            // return (int)value;

            return value.ToInt32(CultureInfo.InvariantCulture);
        }

        public string TranslateToDutchName(TEnum value)
        {
            return DutchTranslations.ContainsKey(value) ? DutchTranslations[value] : throw new NotImplementedException($"Translation not set for {value}");
        }

        public string TranslateToDutchDescription(TEnum value)
        {
            return DutchDescriptions.ContainsKey(value) ? DutchDescriptions[value] : throw new NotImplementedException($"Description not set for {value}");
        }
    }
}
