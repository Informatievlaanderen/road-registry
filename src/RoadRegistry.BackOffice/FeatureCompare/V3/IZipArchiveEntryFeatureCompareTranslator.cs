namespace RoadRegistry.BackOffice.FeatureCompare.V3;

using System.Threading;
using System.Threading.Tasks;
using Uploads;

public interface IZipArchiveEntryFeatureCompareTranslator
{
    Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
