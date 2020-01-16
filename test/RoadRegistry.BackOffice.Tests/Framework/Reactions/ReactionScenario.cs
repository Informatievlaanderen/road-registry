namespace RoadRegistry.BackOffice.Framework.Reactions
{
    using System;
    using Testing;

    public class ReactionScenario
    {
        public ReactionScenario(RecordedEvent[] givens, RecordedEvent[] thens)
        {
            Givens = givens ?? throw new ArgumentNullException(nameof(givens));
            Thens = thens ?? throw new ArgumentNullException(nameof(thens));
        }

        public RecordedEvent[] Givens { get; }
        public RecordedEvent[] Thens { get; }
    }
}