using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Core
{
    using RoadRegistry.Extensions;
    using RoadRegistry.RoadNetwork;
        using RoadRegistry.RoadSegment.ValueObjects;

    public interface IRoadNetworkIdProvider
    {
        Task<AttributeId> NextEuropeanRoadAttributeId();
        Task<GradeSeparatedJunctionId> NextGradeSeparatedJunctionId();
        Task<AttributeId> NextNationalRoadAttributeId();
        Task<AttributeId> NextNumberedRoadAttributeId();
        Task<RoadNodeId> NextRoadNodeId();
        Task<RoadSegmentId> NextRoadSegmentId();
        Func<Task<AttributeId>> NextRoadSegmentLaneAttributeIdProvider(RoadSegmentId roadSegmentId);
        Func<Task<AttributeId>> NextRoadSegmentSurfaceAttributeIdProvider(RoadSegmentId roadSegmentId);
        Func<Task<AttributeId>> NextRoadSegmentWidthAttributeIdProvider(RoadSegmentId roadSegmentId);
        Task<TransactionId> NextTransactionId();
    }

    public class RoadNetworkIdProvider: IRoadNetworkIdProvider
    {
        private readonly IRoadNetworkIdGenerator _idGenerator;
        private readonly IRoadNetworkView _view;

        public RoadNetworkIdProvider(
            IRoadNetworkIdGenerator idGenerator,
            IRoadNetworkView view)
        {
            _idGenerator = idGenerator.ThrowIfNull();
            _view = view.ThrowIfNull();
        }

        public Task<AttributeId> NextEuropeanRoadAttributeId()
        {
            return _idGenerator.NewEuropeanRoadAttributeIdAsync();
        }

        public Task<GradeSeparatedJunctionId> NextGradeSeparatedJunctionId()
        {
            return _idGenerator.NewGradeSeparatedJunctionIdAsync();
        }

        public Task<AttributeId> NextNationalRoadAttributeId()
        {
            return _idGenerator.NewNationalRoadAttributeIdAsync();
        }

        public Task<AttributeId> NextNumberedRoadAttributeId()
        {
            return _idGenerator.NewNumberedRoadAttributeIdAsync();
        }

        public Task<RoadNodeId> NextRoadNodeId()
        {
            return _idGenerator.NewRoadNodeIdAsync();
        }

        public Task<RoadSegmentId> NextRoadSegmentId()
        {
            return _idGenerator.NewRoadSegmentIdAsync();
        }

        public Func<Task<AttributeId>> NextRoadSegmentLaneAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableLaneAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentLaneAttributeIdAsync, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentLaneAttributeIdAsync;
        }

        public Func<Task<AttributeId>> NextRoadSegmentSurfaceAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableSurfaceAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentSurfaceAttributeIdAsync, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentSurfaceAttributeIdAsync;
        }

        public Func<Task<AttributeId>> NextRoadSegmentWidthAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableWidthAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentWidthAttributeIdAsync, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentWidthAttributeIdAsync;
        }

        public Task<TransactionId> NextTransactionId()
        {
            return _idGenerator.NewTransactionIdAsync();
        }

        private sealed class NextReusableAttributeIdProvider
        {
            private readonly Func<Task<AttributeId>> _generateId;
            private readonly IReadOnlyList<AttributeId> _reusableAttributeIdentifiers;
            private int _index;

            public NextReusableAttributeIdProvider(Func<Task<AttributeId>> generateId, IReadOnlyList<AttributeId> reusableAttributeIdentifiers)
            {
                _generateId = generateId;
                _index = 0;
                _reusableAttributeIdentifiers = reusableAttributeIdentifiers;
            }

            public Task<AttributeId> Next()
            {
                return _index < _reusableAttributeIdentifiers.Count
                    ? Task.FromResult(_reusableAttributeIdentifiers[_index++])
                    : _generateId();
            }
        }
    }
}
