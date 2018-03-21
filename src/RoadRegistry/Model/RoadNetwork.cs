namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.AggregateSource;
    
    public class RoadNetwork : AggregateRootEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
    }
}