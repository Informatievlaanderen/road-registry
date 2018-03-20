using System;
using Aiv.Vbr.AggregateSource;

namespace RoadRegistry
{
    public class RoadNetwork : AggregateRootEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
    }
}