namespace Wegenregister.Road
{
    using System;
    using Aiv.Vbr.AggregateSource;
    using Events;
    using ValueObjects;

    public partial class Road : AggregateRootEntity
    {
        public static readonly Func<Road> Factory = () => new Road();

        public static Road Register(RoadId id)
        {
            var road = Factory();
            road.ApplyChange(new RoadWasRegistered(id));
            return road;
        }
    }
}
