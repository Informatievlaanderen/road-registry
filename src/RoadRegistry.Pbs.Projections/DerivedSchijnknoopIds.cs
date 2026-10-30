namespace RoadRegistry.Pbs.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Schema;
using Schema.Records;

// Hands out ids for new afgeleide schijnknopen. The id is needed while the segment is being projected - it is the
// begin or end node of the AfgeleideWegsegmenten written alongside it - so it cannot be left to the insert.
//
// The next id follows the highest one in use: in the table, or added to this context and not saved yet. Nothing is
// kept between batches, so a batch that is rolled back and retried gets the same ids again, and a rebuild, which empties
// the table, starts over at the first id. This relies on there being one writer: the projection runs as a single
// instance, and the backfill refuses to run alongside it.
//
// A caller that already read the highest stored id for this context passes it in, which saves the round trip.
internal static class DerivedSchijnknoopIds
{
    public static async Task<int> HighestStoredAsync(PbsContext context, CancellationToken ct)
    {
        return await context.DerivedSchijnknopen.Select(x => (int?)x.WK_OIDN).MaxAsync(ct) ?? 0;
    }

    public static async Task<int[]> AllocateAsync(PbsContext context, int count, int? highestStored, CancellationToken ct)
    {
        if (count <= 0)
        {
            return [];
        }

        var stored = highestStored ?? await HighestStoredAsync(context, ct);
        var local = context.DerivedSchijnknopen.Local.Select(x => (int?)x.WK_OIDN).Max() ?? 0;
        var first = Math.Max(Math.Max(stored, local) + 1, DerivedSchijnknoopRecord.InitialId);

        return Enumerable.Range(first, count).ToArray();
    }
}
