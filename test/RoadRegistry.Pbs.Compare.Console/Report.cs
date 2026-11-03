namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal enum Finding
{
    /// <summary>Marten holds it as present, PBS has no row: an event PBS never applied.</summary>
    MissingInPbs,

    /// <summary>Marten holds it as removed, PBS still has a row: a removal PBS never applied.</summary>
    StillInPbsAfterRemoval,

    /// <summary>PBS has a row Marten knows nothing about at all.</summary>
    OnlyInPbs,

    /// <summary>Both sides have it, but PBS does not carry the latest state.</summary>
    StaleInPbs
}

internal sealed class Report
{
    private readonly int _maxFindingsPerCheck;
    private readonly List<(string Entity, Finding Finding, int Id, string Detail)> _details = [];
    private readonly Dictionary<(string Entity, Finding Finding), int> _counts = [];
    private readonly Dictionary<string, (long Marten, long Pbs, long Compared)> _scanned = [];
    private readonly List<string> _notes = [];

    public Report(int maxFindingsPerCheck)
    {
        _maxFindingsPerCheck = maxFindingsPerCheck;
    }

    public bool HasFindings => _counts.Values.Any(count => count > 0);

    public void Add(string entity, Finding finding, int id, string detail = "")
    {
        var key = (entity, finding);
        _counts[key] = _counts.GetValueOrDefault(key) + 1;
        if (_counts[key] <= _maxFindingsPerCheck)
        {
            _details.Add((entity, finding, id, detail));
        }
    }

    public void Scanned(string entity, long marten, long pbs, long compared) => _scanned[entity] = (marten, pbs, compared);

    /// <summary>Anything the run could not check, so the summary never reads cleaner than the run actually was.</summary>
    public void Note(string note) => _notes.Add(note);

    public void WriteSummary(TextWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("Gescand");
        writer.WriteLine($"  {"entiteit",-28} {"marten",12} {"pbs",12} {"vergeleken",12}");
        foreach (var (entity, counts) in _scanned)
        {
            writer.WriteLine($"  {entity,-28} {counts.Marten,12:N0} {counts.Pbs,12:N0} {counts.Compared,12:N0}");
        }

        writer.WriteLine();
        writer.WriteLine("Bevindingen");
        if (!HasFindings)
        {
            writer.WriteLine("  geen - elke entiteit bestaat waar ze moet bestaan, is weg waar ze weg moet zijn, en draagt dezelfde state");
        }
        else
        {
            foreach (var ((entity, finding), count) in _counts.Where(x => x.Value > 0).OrderBy(x => x.Key.Entity).ThenBy(x => x.Key.Finding))
            {
                writer.WriteLine($"  {entity,-28} {Describe(finding),-34} {count,12:N0}");
            }
        }

        if (_notes.Count > 0)
        {
            writer.WriteLine();
            writer.WriteLine("Let op");
            foreach (var note in _notes)
            {
                writer.WriteLine($"  {note}");
            }
        }
    }

    public async Task WriteCsvAsync(string path, CancellationToken ct)
    {
        var builder = new StringBuilder("entiteit;bevinding;id;detail\n");
        foreach (var (entity, finding, id, detail) in _details)
        {
            builder.Append(entity).Append(';')
                   .Append(Describe(finding)).Append(';')
                   .Append(id.ToString(CultureInfo.InvariantCulture)).Append(';')
                   .Append(detail.Replace(';', ',')).Append('\n');
        }

        await File.WriteAllTextAsync(path, builder.ToString(), ct);
    }

    public int DetailCount => _details.Count;

    private static string Describe(Finding finding) => finding switch
    {
        Finding.MissingInPbs => "ontbreekt in pbs",
        Finding.StillInPbsAfterRemoval => "nog in pbs na verwijdering",
        Finding.OnlyInPbs => "enkel in pbs",
        Finding.StaleInPbs => "state wijkt af",
        _ => finding.ToString()
    };
}
