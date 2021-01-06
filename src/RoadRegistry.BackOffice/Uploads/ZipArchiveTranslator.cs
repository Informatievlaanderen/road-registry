namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class ZipArchiveTranslator : IZipArchiveTranslator
    {
        private readonly Dictionary<string, IZipArchiveEntryTranslator> _translators;

        public ZipArchiveTranslator(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _translators =
                new Dictionary<string, IZipArchiveEntryTranslator>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {
                        "WEGKNOOP_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<RoadNodeChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
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
                            encoding, DbaseFileHeaderReadBehavior.Default,
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
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new EuropeanRoadChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "ATTNATIONWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<NationalRoadChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new NationalRoadChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "ATTGENUMWEG_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<NumberedRoadChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new NumberedRoadChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "ATTRIJSTROKEN_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<RoadSegmentLaneChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new RoadSegmentLaneChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "ATTWEGBREEDTE_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<RoadSegmentWidthChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new RoadSegmentWidthChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "ATTWEGVERHARDING_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<RoadSegmentSurfaceChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new RoadSegmentSurfaceChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "RLTOGKRUISING_ALL.DBF",
                        new ZipArchiveDbaseEntryTranslator<GradeSeparatedJunctionChangeDbaseRecord>(
                            encoding, DbaseFileHeaderReadBehavior.Default,
                            new GradeSeparatedJunctionChangeDbaseRecordsTranslator()
                        )
                    },
                    {
                        "TRANSACTIEZONES.DBF",
                        new ZipArchiveDbaseEntryTranslator<TransactionZoneDbaseRecord>(
                            encoding, new DbaseFileHeaderReadBehavior(true),
                            new TransactionZoneDbaseRecordsTranslator()
                        )
                    }
                };
        }

        private static readonly string[] TranslationOrder = { // MC Hammer Style
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
    }
}
