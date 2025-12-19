namespace RoadRegistry.RoadSegment;

public partial class RoadSegment
{
    public RoadNodeId? GetOppositeNode(RoadNodeId id)
    {
        if (StartNodeId == id)
        {
            return EndNodeId;
        }

        if (EndNodeId == id)
        {
            return StartNodeId;
        }

        return null;
    }

    public RoadNodeId? GetCommonNode(RoadSegment other)
    {
        if (StartNodeId == other.StartNodeId || StartNodeId == other.EndNodeId)
        {
            return StartNodeId;
        }

        if (EndNodeId == other.StartNodeId || EndNodeId == other.EndNodeId)
        {
            return EndNodeId;
        }

        return null;
    }
}
