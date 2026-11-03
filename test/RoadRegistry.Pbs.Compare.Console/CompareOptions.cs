namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RoadRegistry.BackOffice;

// Everything comes out of the configuration, so debugging without arguments works: appsettings.json, then
// appsettings.{environment}.json, then appsettings.local.json (gitignored - put the real tst credentials there),
// then environment variables, then the command line. Later wins.
internal sealed record CompareOptions
{
    public const string PbsSwitch = "--pbs";
    public const string MartenSwitch = "--marten";
    public const string OutSwitch = "--out";
    public const string ToleranceSwitch = "--tolerance";
    public const string MaxFindingsSwitch = "--max-findings";

    public static readonly string PbsConnectionKey = $"ConnectionStrings:{WellKnownConnectionNames.PbsProjections}";
    public static readonly string MartenConnectionKey = $"ConnectionStrings:{WellKnownConnectionNames.Marten}";
    public const string OutKey = "Compare:Out";
    public const string ToleranceKey = "Compare:Tolerance";
    public const string MaxFindingsKey = "Compare:MaxFindings";

    // Kept so the two environment variables the tool was first documented with keep working.
    private const string PbsEnvironmentVariable = "PBS_CONNECTION";
    private const string MartenEnvironmentVariable = "MARTEN_CONNECTION";

    public static readonly string Usage = $"""
        Compares the PBS read model against the Marten store: does every road segment, road node and junction that
        should exist exist, does it carry the latest state, and is everything that was removed really gone.

        Both connection strings are required. Without arguments they are read from the configuration, which is what
        makes running this from the debugger work:

          appsettings.json                 staat in git - laat de connectiestrings daar leeg
          appsettings.<environment>.json   optioneel
          appsettings.local.json           gitignored - zet hier je tst-connectiestrings
          omgevingsvariabelen              ConnectionStrings__{WellKnownConnectionNames.PbsProjections} /
                                           ConnectionStrings__{WellKnownConnectionNames.Marten},
                                           of {PbsEnvironmentVariable} / {MartenEnvironmentVariable}
          commandolijn                     gaat boven al het bovenstaande

        Zie de README naast dit bestand voor een voorbeeld van appsettings.local.json.

        Usage:
          dotnet run --project test/RoadRegistry.Pbs.Compare.Console -- [options]

        Options
          {PbsSwitch} <connectionstring>     SQL Server met [RoadRegistryPbs]
          {MartenSwitch} <connectionstring>  PostgreSQL met de Marten-store
          {OutSwitch} <pad>                     bevindingen-csv (standaard pbs-compare-findings.csv)
          {ToleranceSwitch} <meter>            tolerantie op de geometrielengte (standaard 0.05)
          {MaxFindingsSwitch} <n>         maximum aantal detailrijen per check (standaard 1000;
                                           de aantallen in de samenvatting blijven volledig)
          --help                           print dit
        """;

    public static readonly IReadOnlyDictionary<string, string> SwitchMappings = new Dictionary<string, string>
    {
        [PbsSwitch] = PbsConnectionKey,
        [MartenSwitch] = MartenConnectionKey,
        [OutSwitch] = OutKey,
        [ToleranceSwitch] = ToleranceKey,
        [MaxFindingsSwitch] = MaxFindingsKey
    };

    public required string PbsConnectionString { get; init; }
    public required string MartenConnectionString { get; init; }
    public string FindingsPath { get; init; } = "pbs-compare-findings.csv";
    public double LengthTolerance { get; init; } = 0.05;
    public int MaxFindingsPerCheck { get; init; } = 1000;

    public static bool TryCreate(IConfiguration configuration, string[] args, out CompareOptions? options, out string? error)
    {
        options = null;

        // AddCommandLine quietly turns an unmapped --switch into a configuration key of its own, so a typo such as
        // --tolerence would be accepted and then ignored. Catch that here instead.
        var unknown = args
            .Where(argument => argument.StartsWith("--", StringComparison.Ordinal))
            .Select(argument => argument.Split('=')[0])
            .FirstOrDefault(argument => !SwitchMappings.ContainsKey(argument) && argument != "--help");
        if (unknown is not null)
        {
            error = $"Unknown option '{unknown}'.";
            return false;
        }

        var pbs = FirstNonEmpty(configuration[PbsConnectionKey], configuration[PbsEnvironmentVariable]);
        var marten = FirstNonEmpty(configuration[MartenConnectionKey], configuration[MartenEnvironmentVariable]);
        if (pbs is null || marten is null)
        {
            var missing = new List<string>();
            if (pbs is null)
            {
                missing.Add(WellKnownConnectionNames.PbsProjections);
            }

            if (marten is null)
            {
                missing.Add(WellKnownConnectionNames.Marten);
            }

            error = $"No connection string for {string.Join(" and ", missing)}. "
                    + "Zet ze in appsettings.local.json naast de binary, of geef ze mee als argument.";
            return false;
        }

        if (!TryReadDouble(configuration, ToleranceKey, 0.05, out var tolerance))
        {
            error = $"{ToleranceSwitch} ({ToleranceKey}) must be a number in metres, e.g. 0.05.";
            return false;
        }

        if (!TryReadInt(configuration, MaxFindingsKey, 1000, out var maxFindings) || maxFindings < 1)
        {
            error = $"{MaxFindingsSwitch} ({MaxFindingsKey}) must be a positive number.";
            return false;
        }

        error = null;
        options = new CompareOptions
        {
            PbsConnectionString = pbs,
            MartenConnectionString = marten,
            FindingsPath = FirstNonEmpty(configuration[OutKey]) ?? "pbs-compare-findings.csv",
            LengthTolerance = tolerance,
            MaxFindingsPerCheck = maxFindings
        };
        return true;
    }

    // An unfilled placeholder in appsettings.json is an empty string, not a missing key, so empty has to count as
    // absent or it would shadow the environment variable behind it.
    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static bool TryReadDouble(IConfiguration configuration, string key, double fallback, out double value)
    {
        var raw = FirstNonEmpty(configuration[key]);
        if (raw is null)
        {
            value = fallback;
            return true;
        }

        return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadInt(IConfiguration configuration, string key, int fallback, out int value)
    {
        var raw = FirstNonEmpty(configuration[key]);
        if (raw is null)
        {
            value = fallback;
            return true;
        }

        return int.TryParse(raw, CultureInfo.InvariantCulture, out value);
    }
}
