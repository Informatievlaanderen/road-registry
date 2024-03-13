namespace RoadRegistry.BackOffice.Messages;

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

        foreach (var change in changes)
            switch (change.Flatten())
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
                case RoadSegmentAdded _:
                    summary.RoadSegments.Added += 1;
                    break;
                case RoadSegmentModified _:
                case RoadSegmentAttributesModified _:
                case RoadSegmentGeometryModified _:
                    summary.RoadSegments.Modified += 1;
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

        return summary;
    }
}
