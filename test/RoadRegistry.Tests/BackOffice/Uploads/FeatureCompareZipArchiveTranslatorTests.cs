namespace RoadRegistry.Tests.BackOffice.Uploads;

using FluentValidation;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.DutchTranslations;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

public class FeatureCompareZipArchiveTranslatorTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IZipArchiveFeatureCompareTranslator _sut;
    private readonly ZipArchiveTranslator _zipArchiveTranslator;

    public FeatureCompareZipArchiveTranslatorTests(ITestOutputHelper outputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger, FileEncoding encoding)
    {
        _outputHelper = outputHelper;
        _sut = ZipArchiveFeatureCompareTranslatorFactory.Create(logger);
        _zipArchiveTranslator = new ZipArchiveTranslator(encoding);
    }

    [Fact]
    public void ArchiveCanNotBeNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Translate(null, CancellationToken.None));
    }

    [Fact(Skip = "For debugging purposes, local feature compare testing")]
    //[Fact]
    public async Task RunZipArchiveFeatureCompareTranslator()
    {
        var pathArchive = @"upload.zip";

        try
        {
            using (var archiveStream = File.OpenRead(pathArchive))
            using (var archive = new ZipArchive(archiveStream))
            {
                var validator = ZipArchiveBeforeFeatureCompareValidatorFactory.Create();
                var archiveValidationProblems = validator.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

                var sw = Stopwatch.StartNew();
                _outputHelper.WriteLine("Started translate Before-FC");
                var translatedChanges = await _sut.Translate(archive, CancellationToken.None);
                _outputHelper.WriteLine($"Finished translate Before-FC at {sw.Elapsed}");

                await ValidateTranslatedChanges(translatedChanges);
            }
        }
        catch (ZipArchiveValidationException ex)
        {
            _outputHelper.WriteLine($"{ex.Problems.Count} problems found:");
            foreach (var problem in ex.Problems)
            {
                var translation = FileProblemTranslator.Dutch(problem.Translate());
                _outputHelper.WriteLine(problem.Describe());
                _outputHelper.WriteLine($"-> {translation.Code}: {translation.Message}");
            }
            throw;
        }
    }

    private async Task ValidateTranslatedChanges(TranslatedChanges translatedChanges)
    {
        var requestedChanges = new List<RequestedChange>();
        foreach (var change in translatedChanges)
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            requestedChanges.Add(requestedChange);
        }

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            DownloadId = new DownloadId(),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, CancellationToken.None);
    }

    [Fact(Skip = "For debugging purposes, local feature compare testing only due to big archive files")]
    //[Fact]
    public async Task TranslateWithRecordsReturnsExpectedResult()
    {
        const string featureCompareArchivesPath = @"\temp\fc-archives";
        const string featureCompareRunnerPath = @"\DV\Repos\road-registry-featurecompare\src\RoadRegistry.FeatureCompare.Runner\bin\Debug\RoadRegistry.FeatureCompare.Runner.exe";

        var featureCompareArchivesPathCompleted = Path.Combine(featureCompareArchivesPath, "_completed");
        var featureCompareArchivesPathError = Path.Combine(featureCompareArchivesPath, "_error");

        Directory.CreateDirectory(featureCompareArchivesPathCompleted);
        Directory.CreateDirectory(featureCompareArchivesPathError);

        var archivesToProcess = Directory
            .GetFiles(featureCompareArchivesPath, "*", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Select(fi =>
            {
                var downloadId = fi.Name.Replace(".zip", "");
                if (Guid.TryParseExact(downloadId, "N", out _))
                {
                    var beforeFcFileName = $"{downloadId}.zip";
                    var afterFcFileName = $"{downloadId}.zip-after.zip";
                    var afterFcFile = Path.Combine(fi.Directory!.FullName, afterFcFileName);

                    var archiveToProcess = new
                    {
                        DownloadId = downloadId,
                        BeforeFcPath = fi.FullName,
                        AfterFcPath = afterFcFile,
                        BeforeFcCompletedPath = Path.Combine(featureCompareArchivesPathCompleted, beforeFcFileName),
                        AfterFcCompletedPath = Path.Combine(featureCompareArchivesPathCompleted, afterFcFileName),
                        BeforeFcErrorPath = Path.Combine(featureCompareArchivesPathError, beforeFcFileName),
                        AfterFcErrorPath = Path.Combine(featureCompareArchivesPathError, afterFcFileName),
                        TxtErrorPath = Path.Combine(featureCompareArchivesPathError, $"{beforeFcFileName}-error.txt")
                    };
                    if (File.Exists(archiveToProcess.BeforeFcCompletedPath) || File.Exists(archiveToProcess.BeforeFcErrorPath))
                    {
                        File.Delete(archiveToProcess.BeforeFcPath);
                        return null;
                    }

                    return archiveToProcess;
                }

                return null;

            })
            .Where(x => x is not null
                        && !File.Exists(x.BeforeFcCompletedPath)
                        && !File.Exists(x.TxtErrorPath))
            .ToArray();

        if (File.Exists(featureCompareRunnerPath))
        {
            foreach (var archiveToProcess in archivesToProcess)
            {
                if (!File.Exists(archiveToProcess.AfterFcPath))
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), archiveToProcess.DownloadId);
                    var tempPathBeforeFc = Path.Combine(tempPath, "before");
                    var tempPathAfterFc = Path.Combine(tempPath, "after");

                    Directory.CreateDirectory(tempPathBeforeFc);
                    Directory.CreateDirectory(tempPathAfterFc);

                    using (var beforeFcArchive = new ZipArchive(File.OpenRead(archiveToProcess.BeforeFcPath)))
                    {
                        beforeFcArchive.ExtractToDirectory(tempPathBeforeFc);
                    }

                    try
                    {
                        await Process.Start(featureCompareRunnerPath, new[]
                        {
                        tempPathBeforeFc, tempPathAfterFc
                    }).WaitForExitAsync();

                        using (var ms = new MemoryStream())
                        {
                            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true, Encoding.UTF8))
                            {
                                var afterFcFiles = Directory.GetFiles(tempPathAfterFc);
                                foreach (var file in afterFcFiles)
                                {
                                    var entry = archive.CreateEntry(new FileInfo(file).Name);
                                    using (var entryStream = entry.Open())
                                    {
                                        await entryStream.WriteAsync(await File.ReadAllBytesAsync(file));
                                    }
                                }
                            }

                            await File.WriteAllBytesAsync(archiveToProcess.AfterFcPath, ms.ToArray());
                        }
                    }
                    finally
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
            }
        }

        foreach (var archiveToProcess in archivesToProcess)
        {
            bool completed;

            var actualChangesJsonPath = $"{archiveToProcess.BeforeFcPath}-events-actual.json";
            var expectedChangesJsonPath = $"{archiveToProcess.BeforeFcPath}-events-expected.json";

            try
            {
                TranslatedChanges expected = null, changes = null;

                if (File.Exists(archiveToProcess.AfterFcPath))
                {
                    using (var archiveStream = File.OpenRead(archiveToProcess.AfterFcPath))
                    using (var archive = new ZipArchive(archiveStream))
                    {
                        var sw = Stopwatch.StartNew();
                        _outputHelper.WriteLine($"{archiveToProcess.DownloadId} started translate After-FC");
                        expected = _zipArchiveTranslator.Translate(archive);
                        _outputHelper.WriteLine($"{archiveToProcess.DownloadId} finished translate After-FC at {sw.Elapsed}");
                        await WriteToFile(expectedChangesJsonPath, expected);
                    }
                }

                using (var archiveStream = File.OpenRead(archiveToProcess.BeforeFcPath))
                using (var archive = new ZipArchive(archiveStream))
                {
                    var sw = Stopwatch.StartNew();
                    _outputHelper.WriteLine($"{archiveToProcess.DownloadId} started translate Before-FC");
                    changes = await _sut.Translate(archive, CancellationToken.None);
                    _outputHelper.WriteLine($"{archiveToProcess.DownloadId} finished translate Before-FC at {sw.Elapsed}");
                    await WriteToFile(actualChangesJsonPath, changes);
                }

                if (expected is not null && changes is not null)
                {
                    Assert.Equal(expected, changes, new TranslatedChangeEqualityComparer(true));
                }

                completed = true;
            }
            catch (Exception ex)
            {
                completed = false;

                await File.WriteAllTextAsync(archiveToProcess.TxtErrorPath, ex.ToString());

                if (File.Exists(expectedChangesJsonPath))
                {
                    File.Move(expectedChangesJsonPath, $"{archiveToProcess.BeforeFcErrorPath}-events-expected.json");
                }
                if (File.Exists(actualChangesJsonPath))
                {
                    File.Move(actualChangesJsonPath, $"{archiveToProcess.BeforeFcErrorPath}-events-actual.json");
                }
                if (File.Exists(archiveToProcess.BeforeFcPath))
                {
                    File.Move(archiveToProcess.BeforeFcPath, archiveToProcess.BeforeFcErrorPath);
                }
                if (File.Exists(archiveToProcess.AfterFcPath))
                {
                    File.Move(archiveToProcess.AfterFcPath, archiveToProcess.AfterFcErrorPath);
                }
            }

            if (completed)
            {
                if (File.Exists(expectedChangesJsonPath))
                {
                    File.Delete(expectedChangesJsonPath);
                }
                if (File.Exists(actualChangesJsonPath))
                {
                    File.Delete(actualChangesJsonPath);
                }

                File.Move(archiveToProcess.BeforeFcPath, archiveToProcess.BeforeFcCompletedPath);
                File.Move(archiveToProcess.AfterFcPath, archiveToProcess.AfterFcCompletedPath);
            }
        }
    }

    private static async Task WriteToFile(string path, TranslatedChanges translatedChanges)
    {
        var content = translatedChanges.Describe(requestedChange =>
        {
            if (requestedChange.AddRoadNode?.Geometry.SpatialReferenceSystemIdentifier == 0)
            {
                requestedChange.AddRoadNode.Geometry.SpatialReferenceSystemIdentifier = 31370;
            }
            if (requestedChange.ModifyRoadNode?.Geometry.SpatialReferenceSystemIdentifier == 0)
            {
                requestedChange.ModifyRoadNode.Geometry.SpatialReferenceSystemIdentifier = 31370;
            }
            if (requestedChange.AddRoadSegment?.Geometry.SpatialReferenceSystemIdentifier == 0)
            {
                requestedChange.AddRoadSegment.Geometry.SpatialReferenceSystemIdentifier = 31370;
            }
            if (requestedChange.ModifyRoadSegment?.Geometry.SpatialReferenceSystemIdentifier == 0)
            {
                requestedChange.ModifyRoadSegment.Geometry.SpatialReferenceSystemIdentifier = 31370;
            }
        });

        await File.WriteAllTextAsync(path, content);
    }
}
