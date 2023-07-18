namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.FeatureCompare.Translators;
using RoadRegistry.BackOffice.Uploads;
using System.Collections.Generic;
using System.IO.Compression;

public class ZipArchiveEntryFeatureCompareTranslateContext: ZipArchiveFeatureReaderContext
{
    public ZipArchive Archive { get; }
    public List<RoadSegmentRecord> RoadSegments { get; }

    public ZipArchiveEntryFeatureCompareTranslateContext(ZipArchive archive, ZipArchiveMetadata metadata)
        : base(metadata)
    {
        Archive = archive;
        RoadSegments = new List<RoadSegmentRecord>();
    }
}

public class RoadSegmentRecord
{
    public RoadSegmentRecord(RecordNumber recordNumber, RoadSegmentFeatureCompareAttributes attributes, RecordType recordType)
    {
        RecordNumber = recordNumber;
        Attributes = attributes;
        RecordType = recordType;
    }

    public RoadSegmentId Id => Attributes.Id;

    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareAttributes Attributes { get; }
    public RecordType RecordType { get; }

    public bool GeometryChanged { get; init; }
    public string CompareIdn { get; init; }
    public RoadSegmentId? TempId { get; set; }
    public FeatureType? FeatureType { get; init; }

    public RoadSegmentId GetNewOrOriginalId()
    {
        return TempId ?? Id;
    }
}
