using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoadRegistry.LegacyImporter
{
    public static class SqlCommandExtensions
    {
        public static async Task ExecuteWith(this SqlCommand command, Action<SqlDataReader> handler)
        {
            using (command)
            { // iterate over all segments
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.IsClosed)
                    {
                        while(await reader.ReadAsync())
                        {
                            handler(reader);
                        }
                    }
                }
            }
        } 

        public static async Task ExecuteWith(this SqlCommand command, Func<SqlDataReader, Task> handler)
        {
            using (command)
            { // iterate over all segments
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.IsClosed)
                    {
                        while(await reader.ReadAsync())
                        {
                            await handler(reader);
                        }
                    }
                }
            }
        } 
    }
}
