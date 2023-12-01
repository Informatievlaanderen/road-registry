namespace RoadRegistry.Tests
{
    using System.Runtime.CompilerServices;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Messages;

    public class FakeRoadNetworkIdGenerator: IRoadNetworkIdGenerator
    {
        private readonly Dictionary<string, int> _idCounters = new();

        private int GetNextValue([CallerMemberName] string name = "")
        {
            _idCounters.TryAdd(name, 0);

            _idCounters[name]++;
            return _idCounters[name];
        }
        private int GetCurrentValue(string name)
        {
            _idCounters.TryAdd(name, 0);

            return _idCounters[name];
        }
        private void SetCurrentValue(string name, int value)
        {
            _idCounters[name] = value;
        }
        private void SetNextValueIfGreaterThanCurrent(string name, int value)
        {
            if (value > GetCurrentValue(name))
            {
                SetCurrentValue(name, value);
            }
        }

        public Task<AttributeId> NewEuropeanRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionId()
        {
            return Task.FromResult(new GradeSeparatedJunctionId(GetNextValue()));
        }

        public Task<AttributeId> NewNationalRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<AttributeId> NewNumberedRoadAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<RoadNodeId> NewRoadNodeId()
        {
            return Task.FromResult(new RoadNodeId(GetNextValue()));
        }

        public Task<RoadSegmentId> NewRoadSegmentId()
        {
            return Task.FromResult(new RoadSegmentId(GetNextValue()));
        }

        public Task<AttributeId> NewRoadSegmentLaneAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<AttributeId> NewRoadSegmentSurfaceAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<AttributeId> NewRoadSegmentWidthAttributeId()
        {
            return Task.FromResult(new AttributeId(GetNextValue()));
        }

        public Task<TransactionId> NewTransactionId()
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
                SetCurrentValue(nameof(NewTransactionId), maxTransactionId);
            }

            foreach (var roadNetworkChangesAccepted in events.OfType<RoadNetworkChangesAccepted>())
            {
                foreach (var change in roadNetworkChangesAccepted.Changes.Flatten())
                {
                    switch (change)
                    {
                        case ImportedRoadNode roadNode:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadNodeId), roadNode.Id);
                            break;
                        case RoadNodeAdded roadNode:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadNodeId), roadNode.Id);
                            break;
                        case ImportedRoadSegment roadSegment:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentId), roadSegment.Id);
                            foreach (var lane  in roadSegment.Lanes)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentLaneAttributeId), lane.AttributeId);
                            }
                            foreach (var surface  in roadSegment.Surfaces)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentSurfaceAttributeId), surface.AttributeId);
                            }
                            foreach (var width  in roadSegment.Widths)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentWidthAttributeId), width.AttributeId);
                            }
                            break;
                        case RoadSegmentAdded roadSegment:
                            SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentId), roadSegment.Id);
                            foreach (var lane  in roadSegment.Lanes)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentLaneAttributeId), lane.AttributeId);
                            }
                            foreach (var surface  in roadSegment.Surfaces)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentSurfaceAttributeId), surface.AttributeId);
                            }
                            foreach (var width  in roadSegment.Widths)
                            {
                                SetNextValueIfGreaterThanCurrent(nameof(NewRoadSegmentWidthAttributeId), width.AttributeId);
                            }
                            break;
                        case RoadSegmentAddedToEuropeanRoad europeanRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewEuropeanRoadAttributeId), europeanRoad.AttributeId);
                            break;
                        case RoadSegmentAddedToNationalRoad nationalRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewNationalRoadAttributeId), nationalRoad.AttributeId);
                            break;
                        case RoadSegmentAddedToNumberedRoad numberedRoad:
                            SetNextValueIfGreaterThanCurrent(nameof(NewNumberedRoadAttributeId), numberedRoad.AttributeId);
                            break;
                        case GradeSeparatedJunctionAdded gradeSeparatedJunction:
                            SetNextValueIfGreaterThanCurrent(nameof(NewGradeSeparatedJunctionId), gradeSeparatedJunction.Id);
                            break;
                    }
                }
            }
        }
    }
}
