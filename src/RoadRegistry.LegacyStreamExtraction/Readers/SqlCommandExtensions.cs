namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    internal static class SqlCommandExtensions
    {
        public static async Task ForEachDataRecord(this SqlCommand command, Action<SqlDataReader> handler)
        {
            using (command)
            using (var reader = await command.ExecuteReaderAsync())
                if (!reader.IsClosed)
                    while (await reader.ReadAsync())
                        handler(reader);
        }
    }
}
