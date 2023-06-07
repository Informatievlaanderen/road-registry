namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class AddRoadSegmentToEuropeanRoadEqualityComparer : IEqualityComparer<AddRoadSegmentToEuropeanRoad>
{
    private readonly bool _ignoreRecordNumber;

    public AddRoadSegmentToEuropeanRoadEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(AddRoadSegmentToEuropeanRoad left, AddRoadSegmentToEuropeanRoad right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.TemporaryAttributeId.Equals(right.TemporaryAttributeId)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(AddRoadSegmentToEuropeanRoad instance)
    {
        throw new NotSupportedException();
    }
}