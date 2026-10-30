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
internal static class DerivedSchijnknoopIds
{
    public static async Task<int[]> AllocateAsync(PbsContext context, int count, CancellationToken ct)
    {
        if (count <= 0)
        {
            return [];
        }

        var highestStored = await context.DerivedSchijnknopen.Select(x => (int?)x.WK_OIDN).MaxAsync(ct);
        var highestLocal = context.DerivedSchijnknopen.Local.Select(x => (int?)x.WK_OIDN).Max();
        var first = Math.Max(Math.Max(highestStored ?? 0, highestLocal ?? 0) + 1, DerivedSchijnknoopRecord.InitialId);

        return Enumerable.Range(first, count).ToArray();
    }
}
