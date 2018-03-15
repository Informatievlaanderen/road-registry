namespace Wegenregister.Road
{
    using Aiv.Vbr.AggregateSource;
    using ValueObjects;

    public interface IRoads : IAsyncRepository<Road, RoadId> { }
}
