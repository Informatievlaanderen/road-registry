using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare
{
    using System.IO;
    using System.IO.Compression;
    using Exceptions;
    using FeatureCompare;
    using Uploads;
    using Xunit.Abstractions;

    public class ZipArchiveFeatureCompareTranslatorTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ZipArchiveFeatureCompareTranslatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        //[Fact]
        [Fact(Skip = "For debugging purposes, run feature compare on a local archive")]
        public async Task RunFeatureCompare()
        {
            var path = @"d7d82ea7f37d4e1db45d313cae4b7600.zip";

            var validator = ZipArchiveBeforeFeatureCompareValidatorFactory.Create();
            var translator = ZipArchiveFeatureCompareTranslatorFactory.Create();

            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var archive = new ZipArchive(fileStream);
                    await validator.ValidateAsync(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty), CancellationToken.None);

                    var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                }
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var problem in ex.Problems)
                {
                    _outputHelper.WriteLine(problem.Describe());
                }

                throw;
            }
        }
    }
}
