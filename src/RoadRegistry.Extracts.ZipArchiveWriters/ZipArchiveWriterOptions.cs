namespace RoadRegistry.Extracts.ZipArchiveWriters;

using BackOffice;

public class ZipArchiveWriterOptions: IHasConfigurationKey
{
    public int RoadSegmentBatchSize { get; set; } = 10000;

    public string GetConfigurationKey()
    {
        return "ZipArchiveWriterOptions";
    }
}
