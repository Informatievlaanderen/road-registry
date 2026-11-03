namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Collections.Generic;
using System.Globalization;

// Connection strings may also come from the environment (PBS_CONNECTION / MARTEN_CONNECTION) so they stay out of the
// shell history.
internal sealed record CompareOptions
{
    public const string Usage = """
        Compares the PBS read model against the Marten store: does every road segment, road node and junction that
        should exist exist, does it carry the latest state, and is everything that was removed really gone.

        Usage:
          dotnet run --project test/RoadRegistry.Pbs.Compare.Console -- [options]

        Connections (required; the environment variable is the fallback)
          --pbs <connectionstring>     SQL Server holding [RoadRegistryPbs]  (env PBS_CONNECTION)
          --marten <connectionstring>  PostgreSQL holding the Marten store   (env MARTEN_CONNECTION)

        Options
          --out <path>                 findings csv (default pbs-compare-findings.csv)
          --tolerance <metres>         geometry length tolerance (default 0.05)
          --max-findings <n>           stop collecting details after n findings per check (default 1000;
                                       the counts in the summary stay complete)
        """;

    public required string PbsConnectionString { get; init; }
    public required string MartenConnectionString { get; init; }
    public string FindingsPath { get; init; } = "pbs-compare-findings.csv";
    public double LengthTolerance { get; init; } = 0.05;
    public int MaxFindingsPerCheck { get; init; } = 1000;

    public static bool TryParse(string[] args, out CompareOptions? options, out string? error)
    {
        options = null;
        error = null;

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
            {
                error = $"Unexpected argument '{key}'.";
                return false;
            }

            if (i + 1 >= args.Length)
            {
                error = $"Option '{key}' needs a value.";
                return false;
            }

            values[key[2..]] = args[++i];
        }

        var pbs = values.GetValueOrDefault("pbs") ?? Environment.GetEnvironmentVariable("PBS_CONNECTION");
        var marten = values.GetValueOrDefault("marten") ?? Environment.GetEnvironmentVariable("MARTEN_CONNECTION");
        if (string.IsNullOrWhiteSpace(pbs) || string.IsNullOrWhiteSpace(marten))
        {
            error = "Both --pbs and --marten are required (or PBS_CONNECTION / MARTEN_CONNECTION).";
            return false;
        }

        var tolerance = 0.05;
        if (values.TryGetValue("tolerance", out var rawTolerance)
            && !double.TryParse(rawTolerance, NumberStyles.Float, CultureInfo.InvariantCulture, out tolerance))
        {
            error = "--tolerance must be a number in metres, e.g. 0.05.";
            return false;
        }

        var maxFindings = 1000;
        if (values.TryGetValue("max-findings", out var rawMax)
            && (!int.TryParse(rawMax, CultureInfo.InvariantCulture, out maxFindings) || maxFindings < 1))
        {
            error = "--max-findings must be a positive number.";
            return false;
        }

        options = new CompareOptions
        {
            PbsConnectionString = pbs,
            MartenConnectionString = marten,
            FindingsPath = values.GetValueOrDefault("out") ?? "pbs-compare-findings.csv",
            LengthTolerance = tolerance,
            MaxFindingsPerCheck = maxFindings
        };
        return true;
    }
}
