namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning.V2;

using RoadRegistry.Extracts;

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

