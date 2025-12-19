namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Models;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Uploads;

public class RoadNodeFeatureCompareRecord
{
    public RoadNodeFeatureCompareRecord(FeatureType featureType, RecordNumber recordNumber, RoadNodeFeatureCompareAttributes attributes, RoadNodeId id, RecordType recordType)
    {
        FeatureType = featureType;
        RecordNumber = recordNumber;
        Attributes = attributes;
        Id = id;
        RecordType = recordType;
    }

    public FeatureType FeatureType { get; }
    public RecordNumber RecordNumber { get; }
    public RoadNodeFeatureCompareAttributes Attributes { get; }
    public RoadNodeId Id { get; set; }
    public RecordType RecordType { get; }

    public RoadNodeId GetActualId() => Id;
    public RoadNodeId GetOriginalId() => Attributes.Id;
}
