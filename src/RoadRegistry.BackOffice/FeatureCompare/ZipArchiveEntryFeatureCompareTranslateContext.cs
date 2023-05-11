namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.FeatureCompare.Translators;
using RoadRegistry.BackOffice.Uploads;
using System.Collections.Generic;
using System.IO.Compression;

public class ZipArchiveEntryFeatureCompareTranslateContext
{
    public IReadOnlyCollection<ZipArchiveEntry> Entries { get; }
    public List<RoadSegmentRecord> RoadSegments { get; }

    public ZipArchiveEntryFeatureCompareTranslateContext(IReadOnlyCollection<ZipArchiveEntry> entries, List<RoadSegmentRecord> roadSegments)
    {
        Entries = entries;
        RoadSegments = roadSegments;
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

    public int Id => Attributes.WS_OIDN;

    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareAttributes Attributes { get; }
    public RecordType RecordType { get; }

    public bool GeometryChanged { get; set; }
    public string CompareIdn { get; set; }
    public int? TempId { get; set; }

    public int GetNewOrOriginalId()
    {
        return TempId ?? Id;
    }
}
