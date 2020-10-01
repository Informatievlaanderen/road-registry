namespace RoadRegistry.BackOffice.Api.Configuration
{
    public class ZipArchiveWriterOptions
    {
        public int RoadSegmentBatchSize { get; set; } = 10000;
    }
}
