namespace RoadRegistry.Tests;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.V1;
using Extracts.Schemas.ExtractV1;
using Extracts.Schemas.ExtractV1.RoadNodes;
using Extracts.Schemas.ExtractV1.RoadSegments;

public static class ExtractV1FixtureExtensions
{
    public static ZipArchive CreateUploadZipArchiveV1(
        this Fixture fixture,
        ExtractV1ZipArchiveTestData testData,
        MemoryStream roadSegmentShapeChangeStream = null,
        MemoryStream roadSegmentProjectionFormatStream = null,
        MemoryStream roadSegmentDbaseChangeStream = null,
        MemoryStream roadNodeShapeChangeStream = null,
        MemoryStream roadNodeProjectionFormatStream = null,
        MemoryStream roadNodeDbaseChangeStream = null,
        MemoryStream europeanRoadChangeStream = null,
        MemoryStream numberedRoadChangeStream = null,
        MemoryStream nationalRoadChangeStream = null,
        MemoryStream laneChangeStream = null,
        MemoryStream widthChangeStream = null,
        MemoryStream surfaceChangeStream = null,
        MemoryStream gradeSeparatedJunctionChangeStream = null,
        MemoryStream roadSegmentShapeExtractStream = null,
        MemoryStream roadSegmentDbaseExtractStream = null,
        MemoryStream roadNodeShapeExtractStream = null,
        MemoryStream roadNodeDbaseExtractStream = null,
        MemoryStream europeanRoadExtractStream = null,
        MemoryStream numberedRoadExtractStream = null,
        MemoryStream nationalRoadExtractStream = null,
        MemoryStream laneExtractStream = null,
        MemoryStream widthExtractStream = null,
        MemoryStream surfaceExtractStream = null,
        MemoryStream gradeSeparatedJunctionExtractStream = null,
        MemoryStream transactionZoneStream = null,
        MemoryStream roadSegmentShapeIntegrationStream = null,
        MemoryStream roadSegmentDbaseIntegrationStream = null,
        MemoryStream roadNodeShapeIntegrationStream = null,
        MemoryStream roadNodeDbaseIntegrationStream = null,
        MemoryStream archiveStream = null,
        ICollection<string> excludeFileNames = null
    )
    {
        var files = new Dictionary<string, Stream>
        {
            { "IWEGSEGMENT.SHP", roadSegmentShapeIntegrationStream ?? fixture.CreateEmptyRoadSegmentShapeFile() },
            { "IWEGSEGMENT.DBF", roadSegmentDbaseIntegrationStream ?? fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema) },
            { "IWEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "EWEGSEGMENT.SHP", roadSegmentShapeExtractStream },
            { "EWEGSEGMENT.DBF", roadSegmentDbaseExtractStream },
            { "EWEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "WEGSEGMENT.SHP", roadSegmentShapeChangeStream },
            { "WEGSEGMENT.DBF", roadSegmentDbaseChangeStream },
            { "WEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "IWEGKNOOP.SHP", roadNodeShapeIntegrationStream ?? fixture.CreateEmptyRoadNodeShapeFile() },
            { "IWEGKNOOP.DBF", roadNodeDbaseIntegrationStream ?? fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema) },
            { "IWEGKNOOP.PRJ", roadNodeProjectionFormatStream },
            { "EWEGKNOOP.SHP", roadNodeShapeExtractStream },
            { "EWEGKNOOP.DBF", roadNodeDbaseExtractStream },
            { "EWEGKNOOP.PRJ", roadNodeProjectionFormatStream },
            { "WEGKNOOP.SHP", roadNodeShapeChangeStream },
            { "WEGKNOOP.DBF", roadNodeDbaseChangeStream },
            { "WEGKNOOP.PRJ", roadNodeProjectionFormatStream },
            { "ATTEUROPWEG.DBF", europeanRoadChangeStream },
            { "EATTEUROPWEG.DBF", europeanRoadExtractStream },
            { "ATTGENUMWEG.DBF", numberedRoadChangeStream },
            { "EATTGENUMWEG.DBF", numberedRoadExtractStream },
            { "ATTNATIONWEG.DBF", nationalRoadChangeStream },
            { "EATTNATIONWEG.DBF", nationalRoadExtractStream },
            { "ATTRIJSTROKEN.DBF", laneChangeStream },
            { "EATTRIJSTROKEN.DBF", laneExtractStream },
            { "ATTWEGBREEDTE.DBF", widthChangeStream },
            { "EATTWEGBREEDTE.DBF", widthExtractStream },
            { "ATTWEGVERHARDING.DBF", surfaceChangeStream },
            { "EATTWEGVERHARDING.DBF", surfaceExtractStream },
            { "RLTOGKRUISING.DBF", gradeSeparatedJunctionChangeStream },
            { "ERLTOGKRUISING.DBF", gradeSeparatedJunctionExtractStream },
            { "TRANSACTIEZONES.DBF", transactionZoneStream ?? fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema) }
        };

        var random = new Random(fixture.Create<int>());
        var fileNames = files.Keys.ToList();
        if (excludeFileNames is not null && excludeFileNames.Any())
        {
            fileNames = fileNames.Where(fileName => !excludeFileNames.Contains(fileName, StringComparer.InvariantCultureIgnoreCase)).ToList();
        }
        var writeOrder = fileNames.OrderBy(_ => random.Next()).ToArray();

        var leaveArchiveStreamOpen = archiveStream is not null;
        archiveStream ??= new MemoryStream();

        var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, leaveArchiveStreamOpen, Encoding.UTF8);
        foreach (var file in writeOrder)
        {
            var stream = files[file];
            if (stream is not null)
            {
                stream.Position = 0;
                using var entryStream = archive.CreateEntry(file).Open();
                stream.CopyTo(entryStream);
            }
            else
            {
                var extractFileEntry = testData.ZipArchiveWithEmptyFiles.Entries.SingleOrDefault(x => string.Equals(x.Name, file, StringComparison.InvariantCultureIgnoreCase));
                if (extractFileEntry is null)
                {
                    throw new Exception($"No file found in {nameof(testData.ZipArchiveWithEmptyFiles)} with name {file}");
                }

                using var extractFileEntryStream = extractFileEntry.Open();
                using var entryStream = archive.CreateEntry(file).Open();
                extractFileEntryStream.CopyTo(entryStream);
            }
        }

        archiveStream.Position = 0;

        return archive;
    }
}
