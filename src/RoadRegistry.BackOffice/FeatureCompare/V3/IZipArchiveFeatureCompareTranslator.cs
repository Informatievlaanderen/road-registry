namespace RoadRegistry.BackOffice.FeatureCompare.V3;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public interface IZipArchiveFeatureCompareTranslator
{
    Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken);
}
