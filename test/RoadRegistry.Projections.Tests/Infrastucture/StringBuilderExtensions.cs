namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLines<T>(
            this StringBuilder builder,
            IEnumerable<T> collection,
            Func<T, string> format)
        {
            foreach (var item in collection)
            {
                builder.AppendLine(format(item));
            }
            return builder;
        }

        public static StringBuilder AppendLines(
            this StringBuilder builder,
            IEnumerable collection,
            Func<object, string> format)
        {
            return builder.AppendLines((IEnumerable<object>)collection, format);
        }
    }
}
