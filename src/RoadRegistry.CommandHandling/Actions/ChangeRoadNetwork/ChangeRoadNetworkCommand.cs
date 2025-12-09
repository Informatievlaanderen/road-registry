namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using GradeSeparatedJunction.Changes;
using RoadNetwork;
using RoadNode.Changes;
using RoadSegment.Changes;

public class ChangeRoadNetworkCommand
{
    public required ICollection<ChangeRoadNetworkCommandItem> Changes { get; set; } = [];
    public required Guid DownloadId { get; set; }
    public required Guid TicketId { get; set; }

    public RoadNetworkChanges ToRoadNetworkChanges(Provenance provenance)
    {
        var roadNetworkChanges = RoadNetworkChanges.Start()
            .WithProvenance(provenance);

        foreach (var change in Changes.Flatten())
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
                case ModifyGradeSeparatedJunctionChange command:
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

public class ChangeRoadNetworkCommandItem
{
    public AddRoadNodeChange? AddRoadNode { get; set; }
    public ModifyRoadNodeChange? ModifyRoadNode { get; set; }
    public RemoveRoadNodeChange? RemoveRoadNode { get; set; }

    public AddRoadSegmentChange? AddRoadSegment { get; set; }
    public ModifyRoadSegmentChange? ModifyRoadSegment { get; set; }
    public RemoveRoadSegmentChange? RemoveRoadSegment { get; set; }
    public AddRoadSegmentToEuropeanRoadChange? AddRoadSegmentToEuropeanRoad { get; set; }
    public AddRoadSegmentToNationalRoadChange? AddRoadSegmentToNationalRoad { get; set; }
    public RemoveRoadSegmentFromEuropeanRoadChange? RemoveRoadSegmentFromEuropeanRoad { get; set; }
    public RemoveRoadSegmentFromNationalRoadChange? RemoveRoadSegmentFromNationalRoad { get; set; }

    public AddGradeSeparatedJunctionChange? AddGradeSeparatedJunction { get; set; }
    public ModifyGradeSeparatedJunctionChange? ModifyGradeSeparatedJunction { get; set; }
    public RemoveGradeSeparatedJunctionChange? RemoveGradeSeparatedJunction { get; set; }
}
