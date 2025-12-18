namespace RoadRegistry.BackOffice.FeatureCompare.V1;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;

public interface IZipArchiveEntryFeatureCompareTranslator
{
    Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
