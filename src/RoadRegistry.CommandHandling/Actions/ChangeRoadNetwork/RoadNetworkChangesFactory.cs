namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using RoadNetwork;
using RoadNetwork.Changes;

public class RoadNetworkChangesFactory
{
    public RoadNetworkChanges Build(ChangeRoadNetworkCommand roadNetworkCommand)
    {
        ArgumentNullException.ThrowIfNull(roadNetworkCommand);

        var roadNetworkChanges = RoadNetworkChanges.Start();

        foreach (var change in roadNetworkCommand.Changes.Flatten()
                     .Select((change, ordinal) => new SortableChange(change, ordinal))
                     .OrderBy(x => x, new RankChangeBeforeTranslation())
                     .Select(x => x.Change))
        {
            switch (change)
            {
                case AddRoadNodeChange command:
                    roadNetworkChanges.Add(command);
                    break;
                // case ModifyRoadNodeChange command:
                //     roadNetworkChanges.Add(command);
                //     break;
                // case RemoveRoadNodeChange command:
                //     roadNetworkChanges.Add(command);
                //     break;
                case AddRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case ModifyRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                case RemoveRoadSegmentChange command:
                    roadNetworkChanges.Add(command);
                    break;
                // case RemoveRoadSegmentsChange command:
                //     translated = translated.Append(Translate(command, organizations));
                //     break;
                // case RemoveOutlinedRoadSegmentChange command:
                //     translated = translated.Append(Translate(command));
                //     break;
                // case RemoveOutlinedRoadSegmentFromRoadNetworkChange command:
                //     translated = translated.Append(Translate(command));
                //     break;
                // case AddRoadSegmentToEuropeanRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromEuropeanRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case AddRoadSegmentToNationalRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromNationalRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case AddRoadSegmentToNumberedRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromNumberedRoadChange command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                case AddGradeSeparatedJunctionChange command:
                    roadNetworkChanges.Add(command);
                    break;
                // case ModifyGradeSeparatedJunctionChange command:
                //     translated = translated.Append(Translate(command, translated));
                //     break;
                // case RemoveGradeSeparatedJunctionChange command:
                //     translated = translated.Append(Translate(command));
                //     break;
            }
        }

        return roadNetworkChanges;
    }

    private sealed class RankChangeBeforeTranslation : IComparer<SortableChange>
    {
        private static readonly Type[] SequenceByTypeOfChange =
        {
            typeof(AddRoadNodeChange),
            typeof(AddRoadSegmentChange),
            // typeof(AddRoadSegmentToEuropeanRoadChange),
            // typeof(AddRoadSegmentToNationalRoadChange),
            // typeof(AddRoadSegmentToNumberedRoadChange),
            typeof(AddGradeSeparatedJunctionChange),
            // typeof(ModifyRoadNodeChange),
            typeof(ModifyRoadSegmentChange),
            // typeof(ModifyGradeSeparatedJunctionChange),
            // typeof(RemoveRoadSegmentFromEuropeanRoadChange),
            // typeof(RemoveRoadSegmentFromNationalRoadChange),
            // typeof(RemoveRoadSegmentFromNumberedRoadChange),
            // typeof(RemoveGradeSeparatedJunctionChange),
            typeof(RemoveRoadSegmentChange),
            // typeof(RemoveRoadNodeChange)
        };

        public int Compare(SortableChange? left, SortableChange? right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var leftRank = Array.IndexOf(SequenceByTypeOfChange, left.Change.GetType());
            if (leftRank == -1)
            {
                throw new InvalidOperationException($"Change of type {left.Change.GetType().Name} is not configured in '{nameof(SequenceByTypeOfChange)}.");
            }

            var rightRank = Array.IndexOf(SequenceByTypeOfChange, right.Change.GetType());
            if (rightRank == -1)
            {
                throw new InvalidOperationException($"Change of type {right.Change.GetType().Name} is not configured in '{nameof(SequenceByTypeOfChange)}.");
            }

            var comparison = leftRank.CompareTo(rightRank);
            return comparison != 0
                ? comparison
                : left.Ordinal.CompareTo(right.Ordinal);
        }
    }

    private sealed class SortableChange
    {
        public SortableChange(object change, int ordinal)
        {
            Ordinal = ordinal;
            Change = change;
        }

        public object Change { get; }
        public int Ordinal { get; }
    }
}
