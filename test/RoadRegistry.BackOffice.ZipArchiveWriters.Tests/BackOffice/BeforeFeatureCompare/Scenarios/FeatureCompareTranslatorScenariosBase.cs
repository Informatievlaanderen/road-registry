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

    protected async Task<(TranslatedChanges, ZipArchiveProblems)> TranslateSucceeds(
        ZipArchive archive,
        IZipArchiveFeatureCompareTranslator translator = null,
        IZipArchiveBeforeFeatureCompareValidator validator = null)
    {
        using (archive)
        {
            validator ??= ZipArchiveBeforeFeatureCompareValidatorFactory.Create();
            var sut = translator ?? ZipArchiveFeatureCompareTranslatorFactory.Create();

            try
            {
                var problems = await validator.ValidateAsync(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty), CancellationToken.None);
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
