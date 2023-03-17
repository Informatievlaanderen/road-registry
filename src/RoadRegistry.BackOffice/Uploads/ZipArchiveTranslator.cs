namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging;

public class ZipArchiveTranslator : IZipArchiveTranslator
{
    private readonly ILogger _logger;

    private static readonly string[] TranslationOrder =
    {
        // MC Hammer Style
        "TRANSACTIEZONES.DBF",
        "WEGKNOOP_ALL.DBF",
        "WEGKNOOP_ALL.SHP",
        "WEGSEGMENT_ALL.DBF",
        "ATTRIJSTROKEN_ALL.DBF",
        "ATTWEGBREEDTE_ALL.DBF",
        "ATTWEGVERHARDING_ALL.DBF",
        "WEGSEGMENT_ALL.SHP",
        "ATTEUROPWEG_ALL.DBF",
        "ATTNATIONWEG_ALL.DBF",
        "ATTGENUMWEG_ALL.DBF",
        "RLTOGKRUISING_ALL.DBF"
    };

    private readonly Dictionary<string, IZipArchiveEntryTranslator> _translators;

    public ZipArchiveTranslator(Encoding encoding, ILogger logger = null)
    {
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));
        _logger = logger;

        //TODO-rik add V2
        _translators =
            new Dictionary<string, IZipArchiveEntryTranslator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.RoadNodeChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.RoadNodeChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "WEGKNOOP_ALL.SHP",
                    new ZipArchiveShapeEntryTranslator(
                        encoding,
                        new RoadNodeChangeShapeRecordsTranslator()
                    )
                },
                {
                    "WEGSEGMENT_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.RoadSegmentChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.RoadSegmentChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "WEGSEGMENT_ALL.SHP",
                    new ZipArchiveShapeEntryTranslator(
                        encoding,
                        new RoadSegmentChangeShapeRecordsTranslator()
                    )
                },
                {
                    "ATTEUROPWEG_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.EuropeanRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.EuropeanRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTNATIONWEG_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.NationalRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.NationalRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTGENUMWEG_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.NumberedRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.NumberedRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTRIJSTROKEN_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.RoadSegmentLaneChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.RoadSegmentLaneChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTWEGBREEDTE_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.RoadSegmentWidthChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.RoadSegmentWidthChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTWEGVERHARDING_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.RoadSegmentSurfaceChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.RoadSegmentSurfaceChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "RLTOGKRUISING_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<Schema.V1.GradeSeparatedJunctionChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new Schema.V1.GradeSeparatedJunctionChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                Schema.V1.TransactionZoneDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Schema.V1.TransactionZoneDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Schema.V1.TransactionZoneDbaseRecordsTranslator())
                            },
                            {
                                Schema.V2.TransactionZoneDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Schema.V2.TransactionZoneDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Schema.V2.TransactionZoneDbaseRecordsTranslator())
                            }
                        })
                }
            };
    }

    public TranslatedChanges Translate(ZipArchive archive)
    {
        if (archive == null)
            throw new ArgumentNullException(nameof(archive));

        var entries = archive
            .Entries
            .Where(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()) != -1)
            .OrderBy(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()))
            .ToArray();

        _logger?.LogInformation("Translating {Count} entries", entries.Length);

        return entries
            .Aggregate(
                TranslatedChanges.Empty,
                (changes, entry) =>
                {
                    var sw = Stopwatch.StartNew();
                    _logger?.LogInformation("Translating entry {Entry}...", entry.FullName);
                    var result = _translators[entry.FullName].Translate(entry, changes);
                    _logger?.LogInformation("Translating entry {Entry} completed in {Elapsed}", entry.FullName, sw.Elapsed);
                    return result;
                });
    }
}
