namespace RoadRegistry.Extracts.FeatureCompare.Inwinning;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts.Uploads;

public interface IZipArchiveFeatureCompareTranslator
{
    Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken);
}
