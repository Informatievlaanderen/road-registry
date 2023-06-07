namespace RoadRegistry.BackOffice.FeatureCompare;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public interface IZipArchiveEntryFeatureCompareTranslator
{
    Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
