namespace RoadRegistry.BackOffice.ShapeFile;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;

public interface IShapefileRecordEnumerator<TDbaseRecord> : IEnumerator<(TDbaseRecord, Geometry)> where TDbaseRecord : DbaseRecord
{
    RecordNumber CurrentRecordNumber { get; }
}
