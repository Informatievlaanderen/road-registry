namespace RoadRegistry.Editor.Projections.DutchTranslations;

using System;
using BackOffice.Messages;
using Schema.RoadNetworkChanges;

public static class AcceptedChanges
{
    public static readonly Converter<BackOffice.Messages.AcceptedChange[], RoadNetworkChangesSummary> Summarize =
        changes =>
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
        };
}