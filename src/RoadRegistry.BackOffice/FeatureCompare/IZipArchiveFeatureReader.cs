namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;
using Core;
using Extracts;
using Translators;
using Uploads;

public interface IZipArchiveFeatureReader<TFeature>
{
    (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context);
}

public class ZipArchiveFeatureReaderContext
{
    public ZipArchiveMetadata ZipArchiveMetadata { get; }
    public VerificationContextTolerances Tolerances { get; }
    public IDictionary<RoadSegmentId, Feature<RoadSegmentFeatureCompareAttributes>> ChangedRoadSegments { get; }
    public IDictionary<RoadNodeId, Feature<RoadNodeFeatureCompareAttributes>> ChangedRoadNodes { get; }

    public ZipArchiveFeatureReaderContext(ZipArchiveMetadata metadata)
    {
        ZipArchiveMetadata = metadata;
        Tolerances = VerificationContextTolerances.Default;
        ChangedRoadSegments = new Dictionary<RoadSegmentId, Feature<RoadSegmentFeatureCompareAttributes>>();
        ChangedRoadNodes = new Dictionary<RoadNodeId, Feature<RoadNodeFeatureCompareAttributes>>();
    }
}
