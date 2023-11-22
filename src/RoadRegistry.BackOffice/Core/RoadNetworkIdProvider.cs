using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Core
{
    //TODO-rik hier een DBcontext variant van maken, wie moet deze kennen? commandhost al zeker en commandhost moet de dbmigrations uitvoeren
    //de lambdas ook

    public interface IRoadNetworkIdGenerator
    {
        Task<AttributeId> NewEuropeanRoadAttributeId();
        Task<GradeSeparatedJunctionId> NewGradeSeparatedJunctionId();
        Task<AttributeId> NewNationalRoadAttributeId();
        Task<AttributeId> NewNumberedRoadAttributeId();
        Task<RoadNodeId> NewRoadNodeId();
        Task<RoadSegmentId> NewRoadSegmentId();
        Task<AttributeId> NewRoadSegmentLaneAttributeId();
        Task<AttributeId> NewRoadSegmentSurfaceAttributeId();
        Task<AttributeId> NewRoadSegmentWidthAttributeId();
        Task<TransactionId> NewTransactionId();
    }

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
            return _idGenerator.NewEuropeanRoadAttributeId();
        }

        public Task<GradeSeparatedJunctionId> NextGradeSeparatedJunctionId()
        {
            return _idGenerator.NewGradeSeparatedJunctionId();
        }

        public Task<AttributeId> NextNationalRoadAttributeId()
        {
            return _idGenerator.NewNationalRoadAttributeId();
        }

        public Task<AttributeId> NextNumberedRoadAttributeId()
        {
            return _idGenerator.NewNumberedRoadAttributeId();
        }

        public Task<RoadNodeId> NextRoadNodeId()
        {
            return _idGenerator.NewRoadNodeId();
        }
        
        public Task<RoadSegmentId> NextRoadSegmentId()
        {
            return _idGenerator.NewRoadSegmentId();
        }

        public Func<Task<AttributeId>> NextRoadSegmentLaneAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableLaneAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentLaneAttributeId, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentLaneAttributeId;
        }

        public Func<Task<AttributeId>> NextRoadSegmentSurfaceAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableSurfaceAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentSurfaceAttributeId, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentSurfaceAttributeId;
        }

        public Func<Task<AttributeId>> NextRoadSegmentWidthAttributeIdProvider(RoadSegmentId roadSegmentId)
        {
            if (_view.SegmentReusableWidthAttributeIdentifiers.TryGetValue(roadSegmentId, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(_idGenerator.NewRoadSegmentWidthAttributeId, reusableAttributeIdentifiers).Next;
            }

            return _idGenerator.NewRoadSegmentWidthAttributeId;
        }
        
        public Task<TransactionId> NextTransactionId()
        {
            return _idGenerator.NewTransactionId();
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
