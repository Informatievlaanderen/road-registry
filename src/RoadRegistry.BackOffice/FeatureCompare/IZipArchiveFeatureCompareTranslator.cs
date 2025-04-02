namespace RoadRegistry.BackOffice.FeatureCompare;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice.Uploads;

public interface IZipArchiveFeatureCompareTranslator
{
    Task<TranslatedChanges> TranslateAsync(ZipArchive archive, CancellationToken cancellationToken);
}
