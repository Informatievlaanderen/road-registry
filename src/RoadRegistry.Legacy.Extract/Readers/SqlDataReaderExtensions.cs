namespace RoadRegistry.Legacy.Extract.Readers;

using System;
using System.IO;
using Microsoft.Data.SqlClient;

internal static class SqlDataReaderExtensions
{
    public static byte[] GetAllBytes(this SqlDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
            return Array.Empty<byte>();

        var buffer = new byte[4096];
        var offset = 0L;

        using (var stream = new MemoryStream())
        {
            var bytesRead = reader.GetBytes(index, offset, buffer, 0, 4096);

            while (bytesRead != 0L)
            {
                stream.Write(buffer, 0, Convert.ToInt32(bytesRead));
                offset += bytesRead;
                bytesRead = reader.GetBytes(index, offset, buffer, 0, 4096);
            }

            return stream.ToArray();
        }
    }

    public static int? GetNullableInt32(this SqlDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? new int?() : reader.GetInt32(index);
    }

    public static string GetNullableString(this SqlDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? null : reader.GetString(index);
    }
}