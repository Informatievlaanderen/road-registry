namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using System.IO.Compression;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;

public sealed class FakeZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
{
    public Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
        return Task.FromResult(TranslatedChanges.Empty);
    }
}
