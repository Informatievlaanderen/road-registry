namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

using System.Text;

public class BeforeFeatureCompareZipArchiveCleaner : CompositeZipArchiveCleaner
{
    public BeforeFeatureCompareZipArchiveCleaner(Encoding encoding)
        : base(
            new RoadSegmentLaneAttributeZipArchiveCleaner(encoding),
            new RoadSegmentSurfaceAttributeZipArchiveCleaner(encoding),
            new RoadSegmentWidthAttributeZipArchiveCleaner(encoding)
        )
    {
    }
}
