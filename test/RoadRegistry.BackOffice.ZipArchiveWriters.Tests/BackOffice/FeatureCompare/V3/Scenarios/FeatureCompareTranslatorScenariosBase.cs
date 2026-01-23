namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Xunit.Sdk;
using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.DomainV2.IZipArchiveFeatureCompareTranslator;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public abstract class FeatureCompareTranslatorScenariosBase
{
    protected ITestOutputHelper TestOutputHelper { get; }
    protected ILogger<ZipArchiveFeatureCompareTranslator> Logger { get; }
    protected readonly FileEncoding Encoding = FileEncoding.UTF8;

    protected FeatureCompareTranslatorScenariosBase(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
    {
        TestOutputHelper = testOutputHelper;
        Logger = logger;
    }

    protected async Task<TranslatedChanges> TranslateSucceeds(
        ZipArchive archive,
        IZipArchiveFeatureCompareTranslator translator = null)
    {
        using (archive)
        {
            var sut = translator ?? ZipArchiveFeatureCompareTranslatorV3Builder.Create();

            try
            {
                return await sut.TranslateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var problem in ex.Problems)
                {
                    TestOutputHelper.WriteLine(problem.Describe());
                }
                throw;
            }
        }
    }

    protected async Task TranslateReturnsExpectedResult(ZipArchive archive, TranslatedChanges expected, IZipArchiveFeatureCompareTranslator translator = null)
    {
        TranslatedChanges result = null;
        try
        {
            result = await TranslateSucceeds(archive, translator);

            Assert.Equal(expected, result);
        }
        catch (EqualException)
        {
            TestOutputHelper.WriteLine($"Expected:\n{expected.Describe()}");
            await File.WriteAllTextAsync("expected.txt", $"Expected:\n{expected.Describe()}");
            TestOutputHelper.WriteLine($"Actual:\n{result?.Describe()}");
            await File.WriteAllTextAsync("actual.txt", $"Actual:\n{result?.Describe()}");
            throw;
        }
    }

    protected static RoadSegmentDynamicAttributeValues<StreetNameLocalId> BuildStreetNameIdAttributes(StreetNameLocalId? leftSideStreetNameId, StreetNameLocalId? rightSideStreetNameId)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(StreetNameLocalId.NotApplicable);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(leftSideStreetNameId.Value);
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(null, RoadSegmentAttributeSide.Left, leftSideStreetNameId ?? StreetNameLocalId.NotApplicable)
            .Add(null, RoadSegmentAttributeSide.Right, rightSideStreetNameId ?? StreetNameLocalId.NotApplicable);
    }
}
