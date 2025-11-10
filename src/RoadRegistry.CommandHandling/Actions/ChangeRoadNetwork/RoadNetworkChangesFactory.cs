namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using GradeSeparatedJunction.Changes;
using RoadNetwork;
using RoadNode.Changes;
using RoadSegment.Changes;

public class RoadNetworkChangesFactory
{
    public RoadNetworkChanges Build(ChangeRoadNetworkCommand roadNetworkCommand)
    {
        ArgumentNullException.ThrowIfNull(roadNetworkCommand);

        var roadNetworkChanges = RoadNetworkChanges.Start();

        foreach (var change in roadNetworkCommand.Changes.Flatten())
        {
            switch (change)
            {
                case AddRoadNodeChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case ModifyRoadNodeChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveRoadNodeChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case AddRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case ModifyRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case AddRoadSegmentToEuropeanRoadChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case AddRoadSegmentToNationalRoadChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case AddGradeSeparatedJunctionChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveGradeSeparatedJunctionChange command:
                    roadNetworkChanges.Add(command);
                    break;
                default:
                    throw new NotImplementedException($"No handler for change '{change.GetType().Name}'");
            }
        }

        return roadNetworkChanges;
    }
}
