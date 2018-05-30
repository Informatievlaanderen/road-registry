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
    }
}
