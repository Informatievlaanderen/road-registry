namespace RoadRegistry.Tests
{
    using System.Runtime.CompilerServices;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Messages;
    using RoadSegment.ValueObjects;
    using RoadNetwork = RoadNetwork.RoadNetwork;

    public class FakeRoadNetworkIdGenerator: IRoadNetworkIdGenerator
    {
        private readonly Dictionary<string, int> _idCounters = new();

        private int GetNextValue([CallerMemberName] string name = "")
        {
            InitializeIdCounter(name);

            _idCounters[name]++;
            return _idCounters[name];
        }

        private int GetCurrentValue(string name)
        {
            InitializeIdCounter(name);

            return _idCounters[name];
        }

        private void SetCurrentValue(string name, int value)
        {
            _idCounters[name] = value;
        }

        private void InitializeIdCounter(string name)
        {
            _idCounters.TryAdd(name, 0);
        }

        private void SetNextValueIfGreaterThanCurrent(string name, int value)
        {
            if (value > GetCurrentValue(name))
            {
                SetCurrentValue(name, value);
            }
        }

        public AttributeId NewEuropeanRoadAttributeId()
        {
            return new AttributeId(GetNextValue());
        }
        public Task<AttributeId> NewEuropeanRoadAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public GradeSeparatedJunctionId NewGradeSeparatedJunctionId()
        {
            return new GradeSeparatedJunctionId(GetNextValue());
        }
        public Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionIdAsync()
        {
            return Task.FromResult(new GradeSeparatedJunctionId(GetNextValue()));
        }

        public AttributeId NewNationalRoadAttributeId()
        {
            return new AttributeId(GetNextValue());
        }
        public Task<AttributeId> NewNationalRoadAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public AttributeId NewNumberedRoadAttributeId()
        {
            return new AttributeId(GetNextValue());
        }
        public Task<AttributeId> NewNumberedRoadAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public RoadNodeId NewRoadNodeId()
        {
            return new RoadNodeId(GetNextValue());
        }
        public Task<RoadNodeId> NewRoadNodeIdAsync()
        {
            return Task.FromResult(new RoadNodeId(GetNextValue()));
        }

        public RoadSegmentId NewRoadSegmentId()
        {
            return new RoadSegmentId(GetNextValue());
        }
        public Task<RoadSegmentId> NewRoadSegmentIdAsync()
        {
            return Task.FromResult(new RoadSegmentId(GetNextValue()));
        }

        public AttributeId NewRoadSegmentLaneAttributeId()
        {
            return new AttributeId(GetNextValue());
        }
        public Task<AttributeId> NewRoadSegmentLaneAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public AttributeId NewRoadSegmentSurfaceAttributeId()
        {
            return new AttributeId(GetNextValue());
        }
        public Task<AttributeId> NewRoadSegmentSurfaceAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public AttributeId NewRoadSegmentWidthAttributeId()
        {
            return new AttributeId(GetNextValue());
        }

        public Task<AttributeId> NewRoadSegmentWidthAttributeIdAsync()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public TransactionId NewTransactionId()
        {
            return new TransactionId(GetNextValue());
        }
        public Task<TransactionId> NewTransactionIdAsync()
        {
            return Task.FromResult(new TransactionId(GetNextValue()));
        }

        internal void SeedEvents(ICollection<object> events)
        {
            var givenTransactionIds = events
                .OfType<IHaveTransactionId>()
                .Select(x => x.TransactionId)
                .ToList();
            if (givenTransactionIds.Any())
            {
                var maxTransactionId = givenTransactionIds.Max();
                SetCurrentValue(nameof(NewTransactionIdAsync), maxTransactionId);
            }

            foreach (var roadNetworkChangesAccepted in events.OfType<RoadNetworkChangesAccepted>())
            {
                foreach (var change in roadNetworkChangesAccepted.Changes.Flatten())
                {
                    switch (change)
                    {
                        case ImportedRoadNode roadNode:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadNodeIdAsync), roadNode.Id);
                            break;
                        case RoadNodeAdded roadNode:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadNodeIdAsync), roadNode.Id);
                            break;
                        case ImportedRoadSegment roadSegment:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentIdAsync), roadSegment.Id);
                            foreach (var lane  in roadSegment.Lanes)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentLaneAttributeIdAsync), lane.AttributeId);
                            }
                            foreach (var surface  in roadSegment.Surfaces)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentSurfaceAttributeIdAsync), surface.AttributeId);
                            }
                            foreach (var width  in roadSegment.Widths)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentWidthAttributeIdAsync), width.AttributeId);
                            }
                            break;
                        case RoadSegmentAdded roadSegment:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentIdAsync), roadSegment.Id);
                            foreach (var lane  in roadSegment.Lanes)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentLaneAttributeIdAsync), lane.AttributeId);
                            }
                            foreach (var surface  in roadSegment.Surfaces)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentSurfaceAttributeIdAsync), surface.AttributeId);
                            }
                            foreach (var width  in roadSegment.Widths)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentWidthAttributeIdAsync), width.AttributeId);
                            }
                            break;
                        case RoadSegmentAddedToEuropeanRoad europeanRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewEuropeanRoadAttributeIdAsync), europeanRoad.AttributeId);
                            break;
                        case RoadSegmentAddedToNationalRoad nationalRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewNationalRoadAttributeIdAsync), nationalRoad.AttributeId);
                            break;
                        case RoadSegmentAddedToNumberedRoad numberedRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewNumberedRoadAttributeIdAsync), numberedRoad.AttributeId);
                            break;
                        case GradeSeparatedJunctionAdded gradeSeparatedJunction:
                            SetNextValueIfGreaterThanCurrent(nameof(NewGradeSeparatedJunctionIdAsync), gradeSeparatedJunction.Id);
                            break;
                    }
                }
            }
        }
    }
}
