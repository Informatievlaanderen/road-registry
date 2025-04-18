namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V1.Scenarios;

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureCompare.V1;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests.BackOffice.Uploads;
using Xunit.Abstractions;
using Xunit.Sdk;

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

    protected async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateSucceeds(
        ZipArchive archive,
        IZipArchiveFeatureCompareTranslator translator = null,
        IZipArchiveBeforeFeatureCompareValidator validator = null)
    {
        using (archive)
        {
            validator ??= ZipArchiveBeforeFeatureCompareValidatorV1Builder.Create();
            var sut = translator ?? ZipArchiveFeatureCompareTranslatorV1Builder.Create();

            try
            {
                var problems = await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);
                problems.ThrowIfError();

                return (await sut.TranslateAsync(archive, CancellationToken.None), problems);
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
            (result, _) = await TranslateSucceeds(archive, translator);

            Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
        }
        catch (EqualException)
        {
            TestOutputHelper.WriteLine($"Expected:\n{expected.Describe()}");
            TestOutputHelper.WriteLine($"Actual:\n{result?.Describe()}");
            throw;
        }
    }
}
