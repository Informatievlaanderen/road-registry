namespace RoadRegistry.Pbs.Projections;

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice;
using Schema;
using Schema.Records;

// Hands out ids for new afgeleide schijnknopen. The id is needed while the segment is being projected - it is the
// begin or end node of the AfgeleideWegsegmenten written alongside it - so it cannot be left to the insert. On SQL
// Server a range of exactly the size asked for is taken from the [AfgeleideSchijnknoopId] sequence, in one round trip
// per segment rebuild, so nothing is handed out that is not used. A batch that is rolled back leaves a gap, which is
// all a sequence ever does.
//
// The in-memory provider has no sequences; there the next id follows the highest one in use, which is what the tests
// need and nothing more.
internal static class DerivedSchijnknoopIds
{
    public static async Task<int[]> AllocateAsync(PbsContext context, int count, CancellationToken ct)
    {
        if (count <= 0)
        {
            return [];
        }

        var first = context.Database.IsRelational()
            ? await NextRangeFromSequence(context, count, ct)
            : await NextRangeInMemory(context, ct);

        return Enumerable.Range(first, count).ToArray();
    }

    private static async Task<int> NextRangeFromSequence(PbsContext context, int count, CancellationToken ct)
    {
        var firstValue = new SqlParameter("@first", SqlDbType.Variant) { Direction = ParameterDirection.Output };
        await context.Database.ExecuteSqlRawAsync(
            $"EXEC sys.sp_sequence_get_range @sequence_name = N'[{WellKnownSchemas.PbsSchema}].[{DerivedSchijnknoopRecord.IdSequenceName}]', @range_size = @size, @range_first_value = @first OUTPUT",
            [new SqlParameter("@size", SqlDbType.BigInt) { Value = (long)count }, firstValue],
            ct);

        return Convert.ToInt32(firstValue.Value);
    }

    private static async Task<int> NextRangeInMemory(PbsContext context, CancellationToken ct)
    {
        var highestStored = await context.DerivedSchijnknopen.Select(x => (int?)x.WK_OIDN).MaxAsync(ct);
        var highestLocal = context.DerivedSchijnknopen.Local.Select(x => (int?)x.WK_OIDN).Max();

        return Math.Max(Math.Max(highestStored ?? 0, highestLocal ?? 0) + 1, DerivedSchijnknoopRecord.InitialId);
    }
}
