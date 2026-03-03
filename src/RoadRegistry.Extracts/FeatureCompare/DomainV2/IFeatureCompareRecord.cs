namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using RoadRegistry.Extracts.Uploads;

public interface IFeatureCompareRecord
{
    RecordType RecordType { get; }
}
