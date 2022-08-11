namespace RoadRegistry.BackOffice.Uploads;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Shaperon;

public static class EnumerableExtensions
{
    public static IDbaseRecordEnumerator<TDbaseRecord> ToDbaseRecordEnumerator<TDbaseRecord>(
        this IEnumerable<TDbaseRecord> enumerable) where TDbaseRecord : DbaseRecord
    {
        return new DbaseRecordEnumerator<TDbaseRecord>(enumerable.GetEnumerator());
    }
}
