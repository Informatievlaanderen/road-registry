namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.Tests.BackOffice.Uploads;
using System.IO.Compression;
using Uploads;
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

    protected async Task<TranslatedChanges> TranslateSucceeds(ZipArchive archive, IZipArchiveFeatureCompareTranslator translator = null)
    {
        using (archive)
        {
            var validator = ZipArchiveBeforeFeatureCompareValidatorFactory.Create();
            var sut = translator ?? ZipArchiveFeatureCompareTranslatorFactory.Create();

            try
            {
                var problems = validator.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));
                problems.ThrowIfError();

                return await sut.Translate(archive, CancellationToken.None);
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
