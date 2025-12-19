namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.GrAr.Common;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Infrastructure.Messages;

public static class RoadNetworkChangeAcceptedExtensions
{
    public static RoadNetworkChangesAccepted WithAcceptedChanges(this RoadNetworkChangesAccepted source, IEnumerable<object> changes)
    {
        return source.WithAcceptedChanges(changes.ToArray());
    }

    public static RoadNetworkChangesAccepted WithAcceptedChanges(this RoadNetworkChangesAccepted source, params object[] changes)
    {
        var acceptedChanges = new List<AcceptedChange>();
        foreach (var change in changes)
        {
            if (change is IHaveHash haveHash)
            {
                var messageHash = haveHash.GetHash();
                if (messageHash == null)
                {
                    throw new InvalidOperationException($"{nameof(haveHash.GetHash)}() returned null");
                }
            }

            var acceptedChange = new AcceptedChange();
            switch (change)
            {
                case RoadNodeAdded roadNodeAdded:
                    acceptedChange.RoadNodeAdded = roadNodeAdded;
                    break;
                case RoadNodeModified roadNodeModified:
                    acceptedChange.RoadNodeModified = roadNodeModified;
                    break;
                case RoadNodeRemoved roadNodeRemoved:
                    acceptedChange.RoadNodeRemoved = roadNodeRemoved;
                    break;
                case RoadSegmentAdded roadSegmentAdded:
                    acceptedChange.RoadSegmentAdded = roadSegmentAdded;
                    break;
                case RoadSegmentModified roadSegmentModified:
                    acceptedChange.RoadSegmentModified = roadSegmentModified;
                    break;
                case RoadSegmentAttributesModified roadSegmentAttributesModified:
                    acceptedChange.RoadSegmentAttributesModified = roadSegmentAttributesModified;
                    break;
                case RoadSegmentGeometryModified roadSegmentGeometryModified:
                    acceptedChange.RoadSegmentGeometryModified = roadSegmentGeometryModified;
                    break;
                case RoadSegmentRemoved roadSegmentRemoved:
                    acceptedChange.RoadSegmentRemoved = roadSegmentRemoved;
                    break;
                case RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                    acceptedChange.RoadSegmentAddedToEuropeanRoad = roadSegmentAddedToEuropeanRoad;
                    break;
                case RoadSegmentRemovedFromEuropeanRoad roadSegmentRemovedFromEuropeanRoad:
                    acceptedChange.RoadSegmentRemovedFromEuropeanRoad = roadSegmentRemovedFromEuropeanRoad;
                    break;
                case RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                    acceptedChange.RoadSegmentAddedToNationalRoad = roadSegmentAddedToNationalRoad;
                    break;
                case RoadSegmentRemovedFromNationalRoad roadSegmentRemovedFromNationalRoad:
                    acceptedChange.RoadSegmentRemovedFromNationalRoad = roadSegmentRemovedFromNationalRoad;
                    break;
                case RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                    acceptedChange.RoadSegmentAddedToNumberedRoad = roadSegmentAddedToNumberedRoad;
                    break;
                case RoadSegmentRemovedFromNumberedRoad roadSegmentRemovedFromNumberedRoad:
                    acceptedChange.RoadSegmentRemovedFromNumberedRoad = roadSegmentRemovedFromNumberedRoad;
                    break;
                case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                    acceptedChange.GradeSeparatedJunctionAdded = gradeSeparatedJunctionAdded;
                    break;
                case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                    acceptedChange.GradeSeparatedJunctionModified = gradeSeparatedJunctionModified;
                    break;
                case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
                    acceptedChange.GradeSeparatedJunctionRemoved = gradeSeparatedJunctionRemoved;
                    break;
                case Problem[] problems:
                    acceptedChange.Problems = problems;
                    break;
                default:
                    throw new ArgumentException($"{change.GetType()} is not supported.");
            }

            acceptedChanges.Add(acceptedChange);
        }

        source.Changes = acceptedChanges.ToArray();

        return source;
    }
}
