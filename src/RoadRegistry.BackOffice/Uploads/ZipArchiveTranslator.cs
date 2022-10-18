namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Schema.V1;

public class ZipArchiveTranslator : IZipArchiveTranslator
{
    public ZipArchiveTranslator(Encoding encoding)
    {
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));

        _translators =
            new Dictionary<string, IZipArchiveEntryTranslator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<RoadNodeChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new RoadNodeChangeDbaseRecordsTranslator()
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
                    new ZipArchiveDbaseEntryTranslator<RoadSegmentChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new RoadSegmentChangeDbaseRecordsTranslator()
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
                    new ZipArchiveDbaseEntryTranslator<EuropeanRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new EuropeanRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTNATIONWEG_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<NationalRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new NationalRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTGENUMWEG_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<NumberedRoadChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new NumberedRoadChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTRIJSTROKEN_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<RoadSegmentLaneChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new RoadSegmentLaneChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTWEGBREEDTE_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<RoadSegmentWidthChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new RoadSegmentWidthChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "ATTWEGVERHARDING_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<RoadSegmentSurfaceChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new RoadSegmentSurfaceChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "RLTOGKRUISING_ALL.DBF",
                    new ZipArchiveDbaseEntryTranslator<GradeSeparatedJunctionChangeDbaseRecord>(
                        encoding, new DbaseFileHeaderReadBehavior(true),
                        new GradeSeparatedJunctionChangeDbaseRecordsTranslator()
                    )
                },
                {
                    "TRANSACTIEZONES.DBF",
                    new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                TransactionZoneDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<TransactionZoneDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new TransactionZoneDbaseRecordsTranslator())
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

    private readonly Dictionary<string, IZipArchiveEntryTranslator> _translators;

    public TranslatedChanges Translate(ZipArchive archive)
    {
        if (archive == null)
            throw new ArgumentNullException(nameof(archive));

        return archive
            .Entries
            .Where(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()) != -1)
            .OrderBy(entry => Array.IndexOf(TranslationOrder, entry.FullName.ToUpperInvariant()))
            .Aggregate(
                TranslatedChanges.Empty,
                (changes, entry) => _translators[entry.FullName].Translate(entry, changes));
    }

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
}
