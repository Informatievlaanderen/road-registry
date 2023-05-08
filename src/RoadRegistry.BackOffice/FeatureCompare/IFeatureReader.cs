namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;

public interface IFeatureReader<TFeature>
{
    List<TFeature> Read(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName);
}
