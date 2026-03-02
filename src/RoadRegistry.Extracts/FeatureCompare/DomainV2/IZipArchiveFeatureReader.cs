namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System.Collections.Generic;
using System.IO.Compression;
using RoadNode;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

public interface IZipArchiveFeatureReader<TFeature>
{
    (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context);
}

public class ZipArchiveFeatureReaderContext
{
    public ZipArchiveMetadata ZipArchiveMetadata { get; }
    public VerificationContextTolerances Tolerances { get; }
    public IDictionary<RoadSegmentTempId, Feature<RoadSegmentFeatureCompareWithFlatAttributes>> ChangedRoadSegments { get; }
    public IDictionary<RoadNodeId, Feature<RoadNodeFeatureCompareAttributes>> ChangedRoadNodes { get; }

    public ZipArchiveFeatureReaderContext(ZipArchiveMetadata metadata)
    {
        ZipArchiveMetadata = metadata;
        Tolerances = VerificationContextTolerances.Default;
        ChangedRoadSegments = new Dictionary<RoadSegmentTempId, Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
        ChangedRoadNodes = new Dictionary<RoadNodeId, Feature<RoadNodeFeatureCompareAttributes>>();
    }
}
