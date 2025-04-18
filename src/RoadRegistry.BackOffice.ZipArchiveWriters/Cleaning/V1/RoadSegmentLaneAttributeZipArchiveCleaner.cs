namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning.V1;

using System.IO.Compression;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

public class RoadSegmentLaneAttributeZipArchiveCleaner : VersionedZipArchiveCleaner
{
    private static readonly ExtractFileName FileName = ExtractFileName.AttRijstroken;

    public RoadSegmentLaneAttributeZipArchiveCleaner(FileEncoding encoding)
        : base(
            new ExtractsDbaseZipArchiveCleaner(encoding, FileName),
            new UploadsV2DbaseZipArchiveCleaner(encoding, FileName),
            new UploadsV1DbaseZipArchiveCleaner(encoding, FileName)
        )
    {
    }

    private sealed class ExtractsDbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<RoadSegmentLaneAttributeDbaseRecord>
    {
        public ExtractsDbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(RoadSegmentLaneAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<RoadSegmentLaneAttributeDbaseRecord> dbfRecords)
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

    private sealed class UploadsV2DbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord>
    {
        public UploadsV2DbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord> dbfRecords)
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

    private sealed class UploadsV1DbaseZipArchiveCleaner : DbaseZipArchiveCleanerBase<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord>
    {
        public UploadsV1DbaseZipArchiveCleaner(FileEncoding encoding, ExtractFileName fileName)
            : base(Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema, encoding, fileName)
        {
        }

        protected override bool FixDataInArchive(ZipArchive archive,
            IReadOnlyCollection<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord> dbfRecords)
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
