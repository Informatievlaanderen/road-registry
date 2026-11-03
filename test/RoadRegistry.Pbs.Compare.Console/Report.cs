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
    // Counted over every finding, not only the ones that fit in the detail cap, so the breakdown stays honest
    // about the whole population while the csv keeps a sample.
    private readonly Dictionary<(string Entity, Finding Finding, string Category), int> _categories = [];
    private readonly Dictionary<string, (long Marten, long Pbs, long Compared)> _scanned = [];
    // The whole Marten population per category, and how much of it PBS actually has. Together with the findings
    // breakdown this says whether a category is partly or entirely absent from PBS - a different diagnosis.
    private readonly Dictionary<(string Entity, string Category), (long Marten, long InPbs)> _population = [];
    private readonly List<string> _notes = [];

    public Report(int maxFindingsPerCheck)
    {
        _maxFindingsPerCheck = maxFindingsPerCheck;
    }

    public bool HasFindings => _counts.Values.Any(count => count > 0);

    public void Add(string entity, Finding finding, int id, string detail = "", string category = "")
    {
        var key = (entity, finding);
        _counts[key] = _counts.GetValueOrDefault(key) + 1;
        if (_counts[key] <= _maxFindingsPerCheck)
        {
            _details.Add((entity, finding, id, detail));
        }

        if (category.Length > 0)
        {
            var categoryKey = (entity, finding, category);
            _categories[categoryKey] = _categories.GetValueOrDefault(categoryKey) + 1;
        }
    }

    public void Scanned(string entity, long marten, long pbs, long compared) => _scanned[entity] = (marten, pbs, compared);

    /// <summary>One Marten row of this category, and whether PBS has it.</summary>
    public void Population(string entity, string category, bool inPbs)
    {
        var key = (entity, category);
        var current = _population.GetValueOrDefault(key);
        _population[key] = (current.Marten + 1, current.InPbs + (inPbs ? 1 : 0));
    }

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

            var breakdown = _population
                .Where(x => x.Key.Entity == entity)
                .OrderByDescending(x => x.Value.Marten);
            foreach (var (key, value) in breakdown)
            {
                var share = value.Marten == 0 ? 0 : 100.0 * value.InPbs / value.Marten;
                writer.WriteLine($"  {string.Empty,-28} {"- " + key.Category,-22} {value.Marten,12:N0} {value.InPbs,12:N0} in pbs ({share,5:F1}%)");
            }
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

                var breakdown = _categories
                    .Where(x => x.Key.Entity == entity && x.Key.Finding == finding)
                    .OrderByDescending(x => x.Value);
                foreach (var (key, categoryCount) in breakdown)
                {
                    writer.WriteLine($"  {string.Empty,-28} {"- " + key.Category,-34} {categoryCount,12:N0}");
                }
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
