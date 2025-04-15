namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning.V1;

public class BeforeFeatureCompareZipArchiveCleaner : CompositeZipArchiveCleaner, IBeforeFeatureCompareZipArchiveCleaner
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

