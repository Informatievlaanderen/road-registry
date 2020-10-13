namespace RoadRegistry
{
    using System;
    using BackOffice.Messages;

    public static class RoadNetworkChangeAcceptedExtensions
    {
        public static RoadNetworkChangesAccepted WithAcceptedChanges(this RoadNetworkChangesAccepted source, params object[] changes)
        {
            var acceptedChange = new AcceptedChange();
            foreach (var change in changes)
            {
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
                    case RoadSegmentOnNumberedRoadModified roadSegmentOnNumberedRoadModified:
                        acceptedChange.RoadSegmentOnNumberedRoadModified = roadSegmentOnNumberedRoadModified;
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
            }

            source.Changes = new[] {acceptedChange};

            return source;
        }
    }
}
