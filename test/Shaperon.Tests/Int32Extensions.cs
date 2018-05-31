namespace Shaperon
{
    using System;

    internal static class Int32Extensions
    {
        public static int AsByteLengthValue(this int value)
        {
            var absolute = Math.Abs(value);
            return absolute == int.MaxValue
                ? 0
                : absolute % 2 == 0
                    ? absolute
                    : absolute + 1;
        }

        public static int AsRecordNumberValue(this int value)
        {
            var absolute = Math.Abs(value);
            return absolute == 0 ? 1 : absolute;
        }

        public static int AsDbaseFieldNameLength(this int value)
        {
            return new Random(value).Next(1, 12);
        }

        public static int AsDbaseFieldLengthValue(this int value)
        {
            return new Random(value).Next(0, 255);
        }
    }
}
