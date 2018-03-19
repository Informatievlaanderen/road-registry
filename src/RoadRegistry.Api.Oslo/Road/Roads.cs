namespace RoadRegistry.Api.Oslo.Road
{
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.AggregateSource.SqlStreamStore;
    using Aiv.Vbr.EventHandling;
    using RoadRegistry.Road;
    using SqlStreamStore;
    using ValueObjects;

    public class Roads : Repository<Road, RoadId>, IRoads
    {
        public Roads(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(Road.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
