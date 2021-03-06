namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Editor.Schema.Extracts;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;

    internal static class ExtractDownloadQueryExtensions
    {
        public static async Task<int> TookAverageAssembleDuration(this DbSet<ExtractDownloadRecord> source, Instant since, int defaultValue)
        {
            // NOTE: This query takes into account all extract downloads that took an hour or less to assemble
            // and computes the median (or the 0.5 or 50th percentile) of that dataset, that is, 50% of the assembly durations
            // took at most the median value (or less). We also limit the data set to anything more recent or equal to the moment
            // identified by the since parameter. Next we compute the average of the resulting set.
            if (await source.AnyAsync())
            {
                return Convert.ToInt32(await source.FromSqlRaw(@"
SELECT DownloadId, ExternalRequestId, RequestId, ArchiveId, RequestedOn, Available, AvailableOn
FROM RoadRegistryEditor.ExtractDownload
WHERE Available = 1
AND RequestedOn >= {0}
AND (AvailableOn - RequestedOn) <= (
    SELECT TOP 1
        PERCENTILE_DISC(0.5)
        WITHIN GROUP (ORDER BY (AvailableOn - RequestedOn))
        OVER ()
    FROM RoadRegistryEditor.ExtractDownload
    WHERE Available = 1
    AND (AvailableOn - RequestedOn) <= 3600)
    AND RequestedOn >= {1}", since.ToUnixTimeSeconds(), since.ToUnixTimeSeconds())
                    .AverageAsync(download => download.AvailableOn - download.RequestedOn));
            }

            return defaultValue;
        }
    }
}
