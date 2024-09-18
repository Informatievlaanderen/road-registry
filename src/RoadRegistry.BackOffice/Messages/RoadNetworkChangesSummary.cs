namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using System.Linq;

public class RoadNetworkChangesSummary
{
    public RoadNetworkChangeCounters GradeSeparatedJunctions { get; set; }
    public RoadNetworkChangeCounters RoadNodes { get; set; }
    public RoadNetworkChangeCounters RoadSegments { get; set; }

    public static RoadNetworkChangesSummary FromAcceptedChanges(AcceptedChange[] changes)
    {
        var summary = new RoadNetworkChangesSummary
        {
            RoadNodes = new RoadNetworkChangeCounters(),
            RoadSegments = new RoadNetworkChangeCounters(),
            GradeSeparatedJunctions = new RoadNetworkChangeCounters()
        };

        var addedRoadSegmentIds = new List<int>();
        var modifiedRoadSegmentIds = new List<int>();

        foreach (var acceptedChange in changes)
            switch (acceptedChange.Flatten())
            {
                case RoadNodeAdded _:
                    summary.RoadNodes.Added += 1;
                    break;
                case RoadNodeModified _:
                    summary.RoadNodes.Modified += 1;
                    break;
                case RoadNodeRemoved _:
                    summary.RoadNodes.Removed += 1;
                    break;
                case RoadSegmentAdded change:
                    addedRoadSegmentIds.Add(change.Id);
                    break;
                case RoadSegmentModified change:
                    modifiedRoadSegmentIds.Add(change.Id);
                    break;
                case RoadSegmentAttributesModified change:
                    modifiedRoadSegmentIds.Add(change.Id);
                    break;
                case RoadSegmentGeometryModified change:
                    modifiedRoadSegmentIds.Add(change.Id);
                    break;
                case RoadSegmentAddedToEuropeanRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentAddedToNationalRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentAddedToNumberedRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentRemovedFromEuropeanRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentRemovedFromNationalRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentRemovedFromNumberedRoad change:
                    if (!addedRoadSegmentIds.Contains(change.SegmentId))
                    {
                        modifiedRoadSegmentIds.Add(change.SegmentId);
                    }
                    break;
                case RoadSegmentRemoved _:
                    summary.RoadSegments.Removed += 1;
                    break;
                case GradeSeparatedJunctionAdded _:
                    summary.GradeSeparatedJunctions.Added += 1;
                    break;
                case GradeSeparatedJunctionModified _:
                    summary.GradeSeparatedJunctions.Modified += 1;
                    break;
                case GradeSeparatedJunctionRemoved _:
                    summary.GradeSeparatedJunctions.Removed += 1;
                    break;
            }

        summary.RoadSegments.Added = addedRoadSegmentIds.Distinct().Count();
        summary.RoadSegments.Modified = modifiedRoadSegmentIds.Distinct().Count();

        return summary;
    }
}
