namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (!CompareOptions.TryParse(args, out var options, out var error))
        {
            if (error is not null)
            {
                Console.Error.WriteLine(error);
                Console.Error.WriteLine();
            }

            Console.WriteLine(CompareOptions.Usage);
            return 1;
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;      // let the run wind down and still print its summary
            cancellation.Cancel();
        };

        var token = cancellation.Token;
        var stopwatch = Stopwatch.StartNew();
        var report = new Report(options!.MaxFindingsPerCheck);

        await using var marten = new NpgsqlConnection(options.MartenConnectionString);
        await using var pbs = new SqlConnection(options.PbsConnectionString);
        await marten.OpenAsync(token);
        await pbs.OpenAsync(token);

        // This process only ever reads, and it must not block the projection hosts it is checking up on.
        await using (var isolation = pbs.CreateCommand())
        {
            isolation.CommandText = "set transaction isolation level read uncommitted";
            await isolation.ExecuteNonQueryAsync(token);
        }

        try
        {
            await Comparers.ComparePositionsAsync(marten, pbs, report, token);
            await Comparers.CompareSegmentsAsync(marten, pbs, options, report, token);
            await Comparers.CompareNodesAsync(marten, pbs, report, token);
            await Comparers.CompareGradeSeparatedJunctionsAsync(marten, pbs, report, token);
            await Comparers.CompareGradeJunctionsAsync(marten, pbs, report, token);
        }
        catch (OperationCanceledException)
        {
            report.Note("de run is afgebroken voor hij klaar was, dus de cijfers hieronder zijn onvolledig");
        }

        report.WriteSummary(Console.Out);

        if (report.DetailCount > 0)
        {
            // Not cancellable: whatever was collected should still land on disk.
            await report.WriteCsvAsync(options.FindingsPath, CancellationToken.None);
            Console.WriteLine();
            Console.WriteLine($"{report.DetailCount:N0} details weggeschreven naar {options.FindingsPath}");
        }

        Console.WriteLine($"Klaar in {stopwatch.Elapsed:hh\\:mm\\:ss}.");

        // 0 = niets gevonden, 2 = bevindingen, zodat dit in een pipeline kan.
        return report.HasFindings ? 2 : 0;
    }
}
