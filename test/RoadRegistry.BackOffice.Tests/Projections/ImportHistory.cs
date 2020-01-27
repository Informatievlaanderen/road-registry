namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Collections.Immutable;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Messages;
    using NodaTime;
    using NodaTime.Text;

    public class ImportHistory
    {
        private readonly IClock _clock;
        private readonly ImmutableList<object> _history;
        private readonly IFixture _fixture;

        private class Context
        {
            public Func<RoadNodeId> NextRoadNodeId { get; }

        }

        private ImportHistory(
            IClock clock,
            ImmutableList<object> history)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        public static ImportHistory At(IClock clock)
        {
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            return new ImportHistory(
                clock,
                ImmutableList<object>.Empty);
        }

        public ImportHistory BeganRoadNetworkImport()
        {
            return new ImportHistory(_clock,
                _history.Add(new BeganRoadNetworkImport
                {
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
        }

        public ImportHistory CompletedRoadNetworkImport()
        {
            return new ImportHistory(_clock,
                _history.Add(new CompletedRoadNetworkImport
                {
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
        }

        public ImportHistory ImportedRoadNode()
        {
            return new ImportHistory(_clock,
                _history.Add(new ImportedRoadNode
                {
                    Id = 123, // next road node
                    Type = RoadNodeType.EndNode,
                    Version = 0, // irrelevant
                    Geometry = new RoadNodeGeometry // next unique geometry
                    {
                        Point = new Messages.Point
                        {
                            X = 0.0, Y = 0.0
                        },
                        SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                    },
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
        }

        public ImportHistory ImportedGradeSeparatedJunction()
        {
            return new ImportHistory(_clock,
                _history.Add(new ImportedGradeSeparatedJunction
                {
                    // TODO
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
        }
    }
}
