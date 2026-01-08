namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using GradeSeparatedJunction.Changes;
using RoadNode.Changes;
using RoadSegment.Changes;
using ScopedRoadNetwork;

public static class ChangeRoadNetworkItemExtensions
{
    public static RoadNetworkChanges ToRoadNetworkChanges(this ICollection<ChangeRoadNetworkItem> changes, ProvenanceData provenance)
    {
        var roadNetworkChanges = RoadNetworkChanges.Start()
            .WithProvenance(provenance.ToProvenance());

        foreach (var change in changes.Select(x => x.Flatten()))
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
