namespace RoadRegistry.BackOffice.ZipArchiveWriters;

public class ZipArchiveWriterOptions
{
    public int RoadSegmentBatchSize { get; set; } = 10000;
}
