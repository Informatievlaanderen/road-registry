namespace RoadRegistry.BackOffice.Handlers.Extensions;

using Editor.Schema.Extracts;
using Microsoft.EntityFrameworkCore;
using NodaTime;

internal static class ExtractQueryExtensions
{
    public static async Task<int> TookAverageAssembleDuration(this DbSet<ExtractDownloadRecord> source, Instant since, int defaultValue)
    {
        // NOTE: This query takes into account all extract downloads that took an hour or less to assemble
        // and computes the median (or the 0.5 or 50th percentile) of that dataset, that is, 50% of the assembly durations
        // took at most the median value (or less). We also limit the data set to anything more recent or equal to the moment
        // identified by the since parameter. Next we compute the average of the resulting set.
        if (await source.AnyAsync())
        {
            var average = await source.FromSqlRaw(@"
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
                .AverageAsync(download => (long?)(download.AvailableOn - download.RequestedOn));
            if (average.HasValue) return Convert.ToInt32(average.Value);
        }

        return defaultValue;
    }

    public static async Task<TimeSpan> TookAverageProcessDuration(this DbSet<ExtractUploadRecord> source, Instant since, TimeSpan defaultValue)
    {
        // NOTE: This query takes into account all extract uploads that took an hour or less to assemble
        // and computes the median (or the 0.5 or 50th percentile) of that dataset, that is, 50% of the assembly durations
        // took at most the median value (or less). We also limit the data set to anything more recent or equal to the moment
        // identified by the since parameter. Next we compute the average of the resulting set.
        if (await source.AnyAsync())
        {
            var average = await source.FromSqlRaw(@"
SELECT UploadId, DownloadId, ExternalRequestId, RequestId, ArchiveId, ChangeRequestId, ReceivedOn, Status, CompletedOn
FROM RoadRegistryEditor.ExtractUpload
WHERE CompletedOn <> 0
AND ReceivedOn >= {0}
AND (CompletedOn - ReceivedOn) <= (
    SELECT TOP 1
        PERCENTILE_DISC(0.5)
        WITHIN GROUP (ORDER BY (CompletedOn - ReceivedOn))
        OVER ()
    FROM RoadRegistryEditor.ExtractUpload
    WHERE CompletedOn <> 0
    AND (CompletedOn - ReceivedOn) <= 3600)
    AND ReceivedOn >= {1}", since.ToUnixTimeSeconds(), since.ToUnixTimeSeconds())
                .AverageAsync(upload => (long?)(upload.CompletedOn - upload.ReceivedOn));
            if (average.HasValue)
            {
                return new TimeSpan(0,0, Convert.ToInt32(average.Value));
            }
        }

        return defaultValue;
    }
}
