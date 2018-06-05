namespace RoadRegistry
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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

        public int TranslateToIdentifier(TEnum value)
        {
            // This would be some much simpler code when "where T : Enum" is supported
            // supported from C# 7.3
            // return (int)value;

            // ReSharper disable once SpecifyACultureInStringConversionExplicitly : <enum>.ToString() does nog have an overload that excepts an IFormatProvider
            return (int)Enum.Parse(typeof(TEnum), value.ToString());
        }

        public string TranslateToDutchName(TEnum value)
        {
            return (DutchTranslations.ContainsKey(value)) ? DutchTranslations[value] : string.Empty;
        }
    }
}
