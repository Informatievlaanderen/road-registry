namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using global::RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;
    using Utilities.HexByteConvertor;

    public class RoadNodeSnapshot : IQueueMessage
    {
        public int Id { get; }
        public int Version { get; }
        public int TypeId { get; }
        public string TypeDutchName { get; }
        public string ExtendedWkbGeometryAsHex { get; }
        public string WktGeometry { get; }
        public int GeometrySrid { get; }
        
        public Origin Origin { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public RoadNodeSnapshot(
            int id,
            int version,
            int typeId,
            string typeDutchName,
            byte[] extendedWkbGeometry,
            string wktGeometry,
            int geometrySrid,
            Origin origin,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            Id = id;
            Version = version;
            TypeId = typeId;
            TypeDutchName = typeDutchName;
            ExtendedWkbGeometryAsHex = extendedWkbGeometry.ToHexString();
            WktGeometry = wktGeometry;
            GeometrySrid = geometrySrid;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
