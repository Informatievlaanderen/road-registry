namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using Utilities.HexByteConvertor;

    public class RoadNodeSnapshot : IQueueMessage
    {
        public int Id { get; }
        public string Type { get; }
        public string ExtendedWkbGeometryAsHex { get; }
        public string WktGeometry { get; }
        public int GeometrySrid { get; }
        public DateTimeOffset? BeginTime { get; }
        public string Organization { get; }
        public DateTimeOffset LastChangedTimestamp { get; }

        public RoadNodeSnapshot(
            int id,
            string type,
            byte[] extendedWkbGeometry,
            string wktGeometry,
            int geometrySrid,
            DateTimeOffset? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            Type = type;
            ExtendedWkbGeometryAsHex = extendedWkbGeometry.ToHexString();
            WktGeometry = wktGeometry;
            GeometrySrid = geometrySrid;
            BeginTime = beginTime;
            Organization = organization;
            LastChangedTimestamp = lastChangedTimestamp;
        }
    }
}
