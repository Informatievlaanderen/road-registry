namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    internal static class SqlCommandExtensions
    {
        public static void ForEachDataRecord(this SqlCommand command, Action<SqlDataReader> handler)
        {
            using (command)
            using (var reader = command.ExecuteReader())
                if (!reader.IsClosed)
                    while (reader.Read())
                        handler(reader);
        }

        public static IEnumerable<T> YieldEachDataRecord<T>(this SqlCommand command, Func<SqlDataReader, T> handler)
        {
            using (command)
            using (var reader = command.ExecuteReader())
                if (!reader.IsClosed)
                    while (reader.Read())
                        yield return handler(reader);
        }
    }
}
