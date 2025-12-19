namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;

public interface IShapeFileRecordEnumerator<TDbaseRecord> : IEnumerator<(TDbaseRecord, Geometry)> where TDbaseRecord : DbaseRecord
{
    RecordNumber CurrentRecordNumber { get; }
}
