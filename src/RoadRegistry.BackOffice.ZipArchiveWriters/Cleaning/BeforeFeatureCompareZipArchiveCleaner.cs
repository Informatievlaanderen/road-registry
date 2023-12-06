namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

public class BeforeFeatureCompareZipArchiveCleaner : CompositeZipArchiveCleaner
{
    public BeforeFeatureCompareZipArchiveCleaner(FileEncoding encoding)
        : base(
            new RoadSegmentLaneAttributeZipArchiveCleaner(encoding),
            new RoadSegmentSurfaceAttributeZipArchiveCleaner(encoding),
            new RoadSegmentWidthAttributeZipArchiveCleaner(encoding)
        )
    {
    }
}
