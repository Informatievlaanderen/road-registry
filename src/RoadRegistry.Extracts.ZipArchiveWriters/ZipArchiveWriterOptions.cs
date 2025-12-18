namespace RoadRegistry.BackOffice.Abstractions;

public class ZipArchiveWriterOptions: IHasConfigurationKey
{
    public int RoadSegmentBatchSize { get; set; } = 10000;

    public string GetConfigurationKey()
    {
        return "ZipArchiveWriterOptions";
    }
}
