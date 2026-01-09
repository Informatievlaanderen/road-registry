namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using GradeSeparatedJunction.Changes;
using RoadNode.Changes;
using RoadSegment.Changes;
using ScopedRoadNetwork;

public class ChangeRoadNetworkItem
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

    public IRoadNetworkChange Flatten()
    {
        return new IRoadNetworkChange[]
                   {
                       AddRoadNode,
                       ModifyRoadNode,
                       RemoveRoadNode,
                       AddRoadSegment,
                       ModifyRoadSegment,
                       RemoveRoadSegment,
                       AddRoadSegmentToEuropeanRoad,
                       RemoveRoadSegmentFromEuropeanRoad,
                       AddRoadSegmentToNationalRoad,
                       RemoveRoadSegmentFromNationalRoad,
                       AddGradeSeparatedJunction,
                       ModifyGradeSeparatedJunction,
                       RemoveGradeSeparatedJunction
                   }
                   .SingleOrDefault(c => !ReferenceEquals(c, null))
               ?? throw new InvalidOperationException($"No change found in {nameof(ChangeRoadNetworkItem)}.");
    }

    public static ChangeRoadNetworkItem Create(IRoadNetworkChange change)
    {
        switch (change)
        {
            case AddRoadNodeChange command:
                return new ChangeRoadNetworkItem
                {
                    AddRoadNode = command
                };
            case ModifyRoadNodeChange command:
                return new ChangeRoadNetworkItem
                {
                    ModifyRoadNode = command
                };
            case RemoveRoadNodeChange command:
                return new ChangeRoadNetworkItem
                {
                    RemoveRoadNode = command
                };
            case AddRoadSegmentChange command:
                return new ChangeRoadNetworkItem
                {
                    AddRoadSegment = command
                };
            case ModifyRoadSegmentChange command:
                return new ChangeRoadNetworkItem
                {
                    ModifyRoadSegment = command
                };
            case RemoveRoadSegmentChange command:
                return new ChangeRoadNetworkItem
                {
                    RemoveRoadSegment = command
                };
            case AddRoadSegmentToEuropeanRoadChange command:
                return new ChangeRoadNetworkItem
                {
                    AddRoadSegmentToEuropeanRoad = command
                };
            case RemoveRoadSegmentFromEuropeanRoadChange command:
                return new ChangeRoadNetworkItem
                {
                    RemoveRoadSegmentFromEuropeanRoad = command
                };
            case AddRoadSegmentToNationalRoadChange command:
                return new ChangeRoadNetworkItem
                {
                    AddRoadSegmentToNationalRoad = command
                };
            case RemoveRoadSegmentFromNationalRoadChange command:
                return new ChangeRoadNetworkItem
                {
                    RemoveRoadSegmentFromNationalRoad = command
                };
            case AddGradeSeparatedJunctionChange command:
                return new ChangeRoadNetworkItem
                {
                    AddGradeSeparatedJunction = command
                };
            case ModifyGradeSeparatedJunctionChange command:
                return new ChangeRoadNetworkItem
                {
                    ModifyGradeSeparatedJunction = command
                };
            case RemoveGradeSeparatedJunctionChange command:
                return new ChangeRoadNetworkItem
                {
                    RemoveGradeSeparatedJunction = command
                };
            default:
                throw new NotImplementedException($"No handler for change '{change.GetType().Name}'");
        }
    }
}
