namespace RoadRegistry.BackOffice.Api.Tests.Uploads;
using Api.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FeatureToggles;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice;

public partial class UploadControllerTests
{
    private static readonly string TransactionZoneDownloadId = DownloadId.Parse("d554de226e6842c597d392a50636fa45").ToString();

    [Fact]
    public async Task When_uploading_a_before_fc_file_that_is_not_a_zip()
    {
        var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(true),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
            formFile,
            CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task When_uploading_a_before_fc_file_with_featuretoggle_disabled()
    {
        var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(false),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(false),
            formFile, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_a_valid_zip()
    {
        await using var sourceStream = await EmbeddedResourceReader.ReadAsync("valid-before.zip");
        var formFile = EmbeddedResourceReader.ReadFormFile(sourceStream, "valid-before.zip", "application/zip");
        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(true),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
            formFile, CancellationToken.None);

        var typedResult = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<UploadExtractFeatureCompareResponseBody>(typedResult.Value);

        Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(response.UploadId)));
        var blob = await UploadBlobClient.GetBlobAsync(new BlobName(response.UploadId));

        await using var openStream = await blob.OpenAsync();
        var resultStream = new MemoryStream();
        await openStream.CopyToAsync(resultStream);
        resultStream.Position = 0;
        sourceStream.Position = 0;

        Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
    }

    [Fact]
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_an_empty_zip()
    {
        try
        {
            await Controller.UploadBeforeFeatureCompare(
                new UseFeatureCompareFeatureToggle(true),
                new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
                await EmbeddedResourceReader.ReadFormFileAsync("empty.zip", "application/zip", CancellationToken.None),
                CancellationToken.None);
            throw new ValidationException("This should not be reachable");
        }
        catch (ZipArchiveValidationException ex)
        {
            var validationFileProblems = ex.Problems.Select(fileProblem => fileProblem.File).ToArray();

            Assert.Contains("WEGKNOOP.SHP", validationFileProblems);
            Assert.Contains("WEGKNOOP.DBF", validationFileProblems);
            Assert.Contains("WEGSEGMENT.SHP", validationFileProblems);
            Assert.Contains("WEGSEGMENT.DBF", validationFileProblems);
            Assert.Contains("ATTEUROPWEG.DBF", validationFileProblems);
            Assert.Contains("ATTNATIONWEG.DBF", validationFileProblems);
            Assert.Contains("ATTGENUMWEG.DBF", validationFileProblems);
            Assert.Contains("ATTRIJSTROKEN.DBF", validationFileProblems);
            Assert.Contains("ATTWEGBREEDTE.DBF", validationFileProblems);
            Assert.Contains("ATTWEGVERHARDING.DBF", validationFileProblems);
            Assert.Contains("RLTOGKRUISING.DBF", validationFileProblems);
            Assert.Contains("TRANSACTIEZONES.DBF", validationFileProblems);
        }
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_vanpos_RoadSegmentLaneAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = null;
            })
            .Build(sourceStream);
        
        await UploadBeforeFeatureCompare(sourceStream);
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentLaneAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = null;
            })
            .Build(sourceStream);
        
        await UploadBeforeFeatureCompare(sourceStream);
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_vanpos_RoadSegmentSurfaceAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value = null;
            })
            .Build(sourceStream);

        await UploadBeforeFeatureCompare(sourceStream);
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentSurfaceAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = null;
            })
            .Build(sourceStream);

        await UploadBeforeFeatureCompare(sourceStream);
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_vanpos_RoadSegmentWidthAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value = null;
            })
            .Build(sourceStream);

        await UploadBeforeFeatureCompare(sourceStream);
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentWidthAttribute()
    {
        var sourceStream = new MemoryStream();
        new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = TransactionZoneDownloadId;

                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = null;
            })
            .Build(sourceStream);

        await UploadBeforeFeatureCompare(sourceStream);
    }

    private async Task<MemoryStream> UploadBeforeFeatureCompare(MemoryStream sourceStream)
    {
        var formFile = EmbeddedResourceReader.ReadFormFile(sourceStream, "archive.zip", "application/zip");
        IActionResult result;
        try
        {
            result = await Controller.UploadBeforeFeatureCompare(
                new UseFeatureCompareFeatureToggle(true),
                new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
                formFile, CancellationToken.None);
        }
        catch (ZipArchiveValidationException ex)
        {
            foreach (var problem in ex.Problems.OfType<FileError>())
            {
                _testOutputHelper.WriteLine(problem.Describe());
            }

            throw;
        }

        var typedResult = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<UploadExtractFeatureCompareResponseBody>(typedResult.Value);

        Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(response.UploadId)));
        var blob = await UploadBlobClient.GetBlobAsync(new BlobName(response.UploadId));

        await using var openStream = await blob.OpenAsync();
        var resultStream = new MemoryStream();
        await openStream.CopyToAsync(resultStream);
        resultStream.Position = 0;
        sourceStream.Position = 0;

        return resultStream;
    }

    private static int CountRequiredFieldIsNull(ZipArchiveProblems problems, string field)
    {
        return problems.Count(x => x.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull)
                                   && x.Parameters.Last().Value == field);
    }
}
