namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts.Uploads;

public interface IZipArchiveEntryFeatureCompareTranslator
{
    Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
