namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

using Extracts;
using Extracts.Dbase.RoadSegments;
using System.IO.Compression;

public class RoadSegmentWidthAttributeZipArchiveCleaner : VersionedZipArchiveCleaner
{
    private static readonly ExtractFileName FileName = ExtractFileName.AttWegbreedte;

    public RoadSegmentWidthAttributeZipArchiveCleaner(FileEncoding encoding)
        : base(
            new ExtractsDbaseZipArchiveCleaner(encoding, FileName),
            new UploadsV2DbaseZipArchiveCleaner(encoding, FileName),
            new UploadsV1DbaseZipArchiveCleaner(encoding, FileName)
        )
    {
    }

    private sealed class ExtractsDbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<RoadSegmentWidthAttributeDbaseRecord>
    {
        public ExtractsDbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(RoadSegmentWidthAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<RoadSegmentWidthAttributeDbaseRecord> dbfRecords)
        {
            return archive.UpdateRoadSegmentAttributeMissingFromOrToPositions(dbfRecords,
                Encoding,
                record => record.WS_OIDN.Value,
                record => record.VANPOS.Value,
                (record, value) => record.VANPOS.Value = value,
                record => record.TOTPOS.Value,
                (record, value) => record.TOTPOS.Value = value);
        }
    }

    private sealed class UploadsV2DbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord>
    {
        public UploadsV2DbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord> dbfRecords)
        {
            return archive.UpdateRoadSegmentAttributeMissingFromOrToPositions(dbfRecords,
                Encoding,
                record => record.WS_OIDN.Value,
                record => record.VANPOS.Value,
                (record, value) => record.VANPOS.Value = value,
                record => record.TOTPOS.Value,
                (record, value) => record.TOTPOS.Value = value);
        }
    }

    private sealed class UploadsV1DbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord>
    {
        public UploadsV1DbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord> dbfRecords)
        {
            return archive.UpdateRoadSegmentAttributeMissingFromOrToPositions(dbfRecords,
                Encoding,
                record => record.WS_OIDN.Value,
                record => record.VANPOS.Value,
                (record, value) => record.VANPOS.Value = value,
                record => record.TOTPOS.Value,
                (record, value) => record.TOTPOS.Value = value);
        }
    }
}
