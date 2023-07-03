namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using System.IO.Compression;
using System.Text;
using Api.Extracts;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Extracts.Dbase;
using BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using BackOffice.Extracts.Dbase.RoadNodes;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using FeatureCompare;
using FeatureCompare.Translators;
using FeatureToggles;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;

public partial class UploadControllerTests
{
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
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentLaneAttribute()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = testData.Fixture;

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord3 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord4 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseIntegrationStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord4 });
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord3 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent3 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent4 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeIntegrationStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent4 });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent3 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        laneDbaseRecord.TOTPOS.Value = null;
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1B.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord1E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1E.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord2B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2B.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2E.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3B.WK_OIDN.Value = roadSegmentDbaseRecord3.B_WK_OIDN.Value;
        var roadNodeDbaseRecord3E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3E.WK_OIDN.Value = roadSegmentDbaseRecord3.E_WK_OIDN.Value;
        var roadNodeDbaseRecord4B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4B.WK_OIDN.Value = roadSegmentDbaseRecord4.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4E.WK_OIDN.Value = roadSegmentDbaseRecord4.E_WK_OIDN.Value;
        var roadNodeDbaseIntegrationStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord4B, roadNodeDbaseRecord4E });
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord3B, roadNodeDbaseRecord3E });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1B, roadNodeDbaseRecord1E, roadNodeDbaseRecord2B, roadNodeDbaseRecord2E });

        var roadNodeShapeContent1B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent1E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4E = fixture.Create<PointShapeContent>();
        var roadNodeShapeIntegrationStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent4B, roadNodeShapeContent4E });
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent3B, roadNodeShapeContent3E });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1B, roadNodeShapeContent1E, roadNodeShapeContent2B, roadNodeShapeContent2E });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema, record => { record.DOWNLOADID.Value = DownloadId.Parse("d554de226e6842c597d392a50636fa45").ToString(); });

        var sourceStream = new MemoryStream();
        var archive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            europeanRoadChangeStream,
            numberedRoadChangeStream,
            nationalRoadChangeStream,
            laneChangeStream,
            widthChangeStream,
            surfaceChangeStream,
            gradeSeparatedJunctionChangeStream,
            transactionZoneStream: transactionZoneStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeIntegrationStream: roadSegmentShapeIntegrationStream,
            roadSegmentDbaseIntegrationStream: roadSegmentDbaseIntegrationStream,
            roadNodeShapeIntegrationStream: roadNodeShapeIntegrationStream,
            roadNodeDbaseIntegrationStream: roadNodeDbaseIntegrationStream,
            archiveStream: sourceStream
        );

        var featureReader = new RoadSegmentLaneFeatureCompareFeatureReader(Encoding.UTF8);
        var fileName = ExtractFileName.AttRijstroken;

        {
            var features = featureReader.Read(archive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(1, nullToPositionCount);
        }

        var resultStream = await UploadBeforeFeatureCompare(sourceStream);

        using (var resultArchive = new ZipArchive(resultStream, ZipArchiveMode.Read, false))
        {
            var features = featureReader.Read(resultArchive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(0, nullToPositionCount);
        }
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentSurfaceAttribute()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = testData.Fixture;

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord3 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord4 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseIntegrationStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord4 });
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord3 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent3 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent4 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeIntegrationStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent4 });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent3 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });
        
        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        surfaceDbaseRecord.TOTPOS.Value = null;
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1B.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord1E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1E.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord2B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2B.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2E.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3B.WK_OIDN.Value = roadSegmentDbaseRecord3.B_WK_OIDN.Value;
        var roadNodeDbaseRecord3E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3E.WK_OIDN.Value = roadSegmentDbaseRecord3.E_WK_OIDN.Value;
        var roadNodeDbaseRecord4B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4B.WK_OIDN.Value = roadSegmentDbaseRecord4.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4E.WK_OIDN.Value = roadSegmentDbaseRecord4.E_WK_OIDN.Value;
        var roadNodeDbaseIntegrationStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord4B, roadNodeDbaseRecord4E });
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord3B, roadNodeDbaseRecord3E });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1B, roadNodeDbaseRecord1E, roadNodeDbaseRecord2B, roadNodeDbaseRecord2E });

        var roadNodeShapeContent1B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent1E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4E = fixture.Create<PointShapeContent>();
        var roadNodeShapeIntegrationStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent4B, roadNodeShapeContent4E });
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent3B, roadNodeShapeContent3E });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1B, roadNodeShapeContent1E, roadNodeShapeContent2B, roadNodeShapeContent2E });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema, record =>
        {
            record.DOWNLOADID.Value = DownloadId.Parse("d554de226e6842c597d392a50636fa45").ToString();
        });

        var sourceStream = new MemoryStream();
        var archive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            europeanRoadChangeStream,
            numberedRoadChangeStream,
            nationalRoadChangeStream,
            laneChangeStream,
            widthChangeStream,
            surfaceChangeStream,
            gradeSeparatedJunctionChangeStream,
            transactionZoneStream: transactionZoneStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeIntegrationStream: roadSegmentShapeIntegrationStream,
            roadSegmentDbaseIntegrationStream: roadSegmentDbaseIntegrationStream,
            roadNodeShapeIntegrationStream: roadNodeShapeIntegrationStream,
            roadNodeDbaseIntegrationStream: roadNodeDbaseIntegrationStream,
            archiveStream: sourceStream
        );

        var featureReader = new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding.UTF8);
        var fileName = ExtractFileName.AttWegverharding;

        {
            var features = featureReader.Read(archive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(1, nullToPositionCount);
        }

        var resultStream = await UploadBeforeFeatureCompare(sourceStream);

        using (var resultArchive = new ZipArchive(resultStream, ZipArchiveMode.Read, false))
        {
            var features = featureReader.Read(resultArchive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(0, nullToPositionCount);
        }
    }

    [Fact]
    public async Task When_uploading_before_fc_zip_with_null_totpos_RoadSegmentWidthAttribute()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = testData.Fixture;

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord3 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord4 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseIntegrationStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord4 });
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord3 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent3 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent4 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeIntegrationStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent4 });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent3 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });
        
        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        widthDbaseRecord.TOTPOS.Value = null;
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1B.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord1E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1E.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord2B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2B.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2E.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3B.WK_OIDN.Value = roadSegmentDbaseRecord3.B_WK_OIDN.Value;
        var roadNodeDbaseRecord3E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3E.WK_OIDN.Value = roadSegmentDbaseRecord3.E_WK_OIDN.Value;
        var roadNodeDbaseRecord4B = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4B.WK_OIDN.Value = roadSegmentDbaseRecord4.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4E = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4E.WK_OIDN.Value = roadSegmentDbaseRecord4.E_WK_OIDN.Value;
        var roadNodeDbaseIntegrationStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord4B, roadNodeDbaseRecord4E });
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord3B, roadNodeDbaseRecord3E });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1B, roadNodeDbaseRecord1E, roadNodeDbaseRecord2B, roadNodeDbaseRecord2E });

        var roadNodeShapeContent1B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent1E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3E = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4B = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4E = fixture.Create<PointShapeContent>();
        var roadNodeShapeIntegrationStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent4B, roadNodeShapeContent4E });
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent3B, roadNodeShapeContent3E });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1B, roadNodeShapeContent1E, roadNodeShapeContent2B, roadNodeShapeContent2E });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema, record =>
        {
            record.DOWNLOADID.Value = DownloadId.Parse("d554de226e6842c597d392a50636fa45").ToString();
        });

        var sourceStream = new MemoryStream();
        var archive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            europeanRoadChangeStream,
            numberedRoadChangeStream,
            nationalRoadChangeStream,
            laneChangeStream,
            widthChangeStream,
            surfaceChangeStream,
            gradeSeparatedJunctionChangeStream,
            transactionZoneStream: transactionZoneStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeIntegrationStream: roadSegmentShapeIntegrationStream,
            roadSegmentDbaseIntegrationStream: roadSegmentDbaseIntegrationStream,
            roadNodeShapeIntegrationStream: roadNodeShapeIntegrationStream,
            roadNodeDbaseIntegrationStream: roadNodeDbaseIntegrationStream,
            archiveStream: sourceStream
        );

        var featureReader = new RoadSegmentWidthFeatureCompareFeatureReader(Encoding.UTF8);
        var fileName = ExtractFileName.AttWegbreedte;

        {
            var features = featureReader.Read(archive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(1, nullToPositionCount);
        }

        var resultStream = await UploadBeforeFeatureCompare(sourceStream);

        using (var resultArchive = new ZipArchive(resultStream, ZipArchiveMode.Read, false))
        {
            var features = featureReader.Read(resultArchive.Entries, FeatureType.Change, fileName);
            var nullToPositionCount = features.Count(x => x.Attributes.ToPosition is null);

            Assert.Equal(0, nullToPositionCount);
        }
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

}
