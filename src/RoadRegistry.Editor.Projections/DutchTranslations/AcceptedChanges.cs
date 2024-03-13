namespace RoadRegistry.Editor.Projections.DutchTranslations;

using System;
using RoadNetworkChangeCounters = Schema.RoadNetworkChanges.RoadNetworkChangeCounters;
using RoadNetworkChangesSummary = Schema.RoadNetworkChanges.RoadNetworkChangesSummary;

public static class AcceptedChanges
{
    public static readonly Converter<BackOffice.Messages.AcceptedChange[], RoadNetworkChangesSummary> Summarize = changes =>
    {
        var summary = BackOffice.Messages.RoadNetworkChangesSummary.FromAcceptedChanges(changes);

        return new RoadNetworkChangesSummary
        {
            RoadNodes = new RoadNetworkChangeCounters
            {
                Added = summary.RoadNodes.Added,
                Modified = summary.RoadNodes.Modified,
                Removed = summary.RoadNodes.Removed
            },
            RoadSegments = new RoadNetworkChangeCounters
            {
                Added = summary.RoadSegments.Added,
                Modified = summary.RoadSegments.Modified,
                Removed = summary.RoadSegments.Removed
            },
            GradeSeparatedJunctions = new RoadNetworkChangeCounters
            {
                Added = summary.GradeSeparatedJunctions.Added,
                Modified = summary.GradeSeparatedJunctions.Modified,
                Removed = summary.GradeSeparatedJunctions.Removed
            },
        };
    };
}
