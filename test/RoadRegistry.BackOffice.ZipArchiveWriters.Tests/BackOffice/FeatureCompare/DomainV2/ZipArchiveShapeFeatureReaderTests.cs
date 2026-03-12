namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
using RoadRegistry.Extracts.Uploads;

public class ZipArchiveShapeFeatureReaderTests
{
    [Fact]
    public async Task WhenShpAndDbfFilesHaveSameNumberOfRecords_ThenAllRecordsAreRead()
    {
        // Arrange
        const FeatureType featureType = FeatureType.Extract;
        var reader = new TestReader(Encoding.UTF8, ExtractFileName.Wegknoop, RoadNodeDbaseRecord.Schema);

        var shpFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\valid\\eWegknoop.shp");
        var dbfFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\valid\\eWegknoop.dbf");
        var prjFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\valid\\eWegknoop.prj");

        using var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update, false);

        await using (var shpEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToShapeFileName(featureType)).Open())
        {
            await shpFile.CopyToAsync(shpEntry);
        }
        await using (var dbfEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToDbaseFileName(featureType)).Open())
        {
            await dbfFile.CopyToAsync(dbfEntry);
        }
        await using (var prjEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToProjectionFileName(featureType)).Open())
        {
            await prjFile.CopyToAsync(prjEntry);
        }

        // Act
        var (features, _) = reader.Read(archive, featureType, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

        // Assert
        features.Should().HaveCount(2);
    }

    [Fact]
    public async Task WhenDbfFileHasMoreRecordsThanShpFile_ThenError()
    {
        // Arrange
        const FeatureType featureType = FeatureType.Extract;
        var reader = new TestReader(Encoding.UTF8, ExtractFileName.Wegknoop, RoadNodeDbaseRecord.Schema);

        var shpFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\dbf-more-than-shp\\eWegknoop.shp");
        var dbfFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\dbf-more-than-shp\\eWegknoop.dbf");
        var prjFile = await EmbeddedResourceReader.ReadAsync("ShapeFeatureReader\\dbf-more-than-shp\\eWegknoop.prj");

        using var archive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update, false);

        await using (var shpEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToShapeFileName(featureType)).Open())
        {
            await shpFile.CopyToAsync(shpEntry);
        }
        await using (var dbfEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToDbaseFileName(featureType)).Open())
        {
            await dbfFile.CopyToAsync(dbfEntry);
        }
        await using (var prjEntry = archive.CreateEntry(ExtractFileName.Wegknoop.ToProjectionFileName(featureType)).Open())
        {
            await prjFile.CopyToAsync(prjEntry);
        }

        // Act
        var (features, problems) = reader.Read(archive, featureType, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

        // Assert
        features.Should().HaveCount(2);
        problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.DbaseRecordHasNoGeometry)
                                       && x.GetParameterValue("RecordNumber") == "3");
    }

    [Fact(Skip = "Unable to reproduce this situation without corrupting the entire dbf/shp file")]
    public void WhenShpFileHasMoreRecordsThanDbfFile_ThenError()
    {
    }

    private sealed class TestReader : ZipArchiveShapeFeatureReader<RoadNodeDbaseRecord, TestFeature>
    {
        public TestReader(Encoding encoding, ExtractFileName fileName, DbaseSchema dbaseSchema, bool treatHasNoRecordsAsError = false)
            : base(encoding, fileName, dbaseSchema, treatHasNoRecordsAsError)
        {
        }

        protected override (TestFeature, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadNodeDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return (new TestFeature
            {
                Id = dbaseRecord.WK_OIDN.Value,
                Geometry = geometry
            }, ZipArchiveProblems.None);
        }
    }

    private sealed class TestFeature
    {
        public required int Id { get; init; }
        public required Geometry Geometry { get; init; }
    }
}
