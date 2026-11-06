namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using System.IO.Compression;
using RoadRegistry.Extracts.FeatureCompare.Inwinning;
using RoadRegistry.Extracts.Uploads;

public sealed class FakeInwinningZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
{
    public Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
        return Task.FromResult(TranslatedChanges.Empty);
    }
}
