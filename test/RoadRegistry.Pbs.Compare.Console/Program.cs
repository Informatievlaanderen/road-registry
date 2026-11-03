namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h"))
        {
            Console.WriteLine(CompareOptions.Usage);
            return 0;
        }

        var configuration = BuildConfiguration(args);
        if (!CompareOptions.TryCreate(configuration, args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine();
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

        // Not reachable is the normal failure here - the tst databases usually need bastion or vpn - so say that in
        // one line instead of unwinding a stack trace over it.
        try
        {
            await marten.OpenAsync(token);
        }
        catch (Exception exception) when (exception is DbException or TimeoutException)
        {
            Console.Error.WriteLine($"Kan niet verbinden met de Marten-databank: {exception.Message}");
            return 1;
        }

        try
        {
            await pbs.OpenAsync(token);
        }
        catch (Exception exception) when (exception is DbException or TimeoutException)
        {
            Console.Error.WriteLine($"Kan niet verbinden met de PBS-databank: {exception.Message}");
            return 1;
        }

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

    // Read from next to the binary rather than from the current directory, so the debugger and `dotnet run` (which
    // start in different places) both end up with the same files. The json files are copied to the output on build.
    private static IConfigurationRoot BuildConfiguration(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "development";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args, new Dictionary<string, string>(CompareOptions.SwitchMappings))
            .Build();
    }
}
