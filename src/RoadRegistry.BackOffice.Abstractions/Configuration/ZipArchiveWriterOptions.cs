namespace RoadRegistry.BackOffice.Abstractions;

public class ZipArchiveWriterOptions
{
    public int RoadSegmentBatchSize { get; set; } = 10000;
}
