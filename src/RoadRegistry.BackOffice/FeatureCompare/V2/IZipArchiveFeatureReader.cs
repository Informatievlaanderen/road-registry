namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using System.Collections.Generic;
using System.IO.Compression;
using Models;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;
using RoadRegistry.BackOffice.Uploads;

public interface IZipArchiveFeatureReader<TFeature>
{
    (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context);
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
