namespace RoadRegistry.RoadNetwork;

public partial class RoadNetwork
{
    public void Change(object command)
    {
        // produce change started event?

        object[] changes = null!;

        // dit vervangt the RequestedChangeTranslator
        foreach (var change in changes)
        {
            switch(change)
            {
                case object modifyRoadSegmentCommand:
                    ModifyRoadSegment(modifyRoadSegmentCommand);
                    break;
                // other cases
            }
        }

        // produce change completed event + commit events to entities (roadsegment,...)
    }
}
