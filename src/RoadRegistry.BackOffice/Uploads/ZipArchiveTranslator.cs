namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.AfterFeatureCompare.V2.Schema;
using Dbase.AfterFeatureCompare.V2.Validation;
using Microsoft.Extensions.Logging;

public class ZipArchiveTranslator : IZipArchiveTranslator
{
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

    private readonly ILogger _logger;
    private readonly Dictionary<string, IZipArchiveEntryTranslator> _translators;

    public ZipArchiveTranslator(Encoding encoding, ILogger logger = null)
    {
        if (encoding == null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        _logger = logger;

        _translators =
            new Dictionary<string, IZipArchiveEntryTranslator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    "WEGKNOOP_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                RoadNodeChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<RoadNodeChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new RoadNodeChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.RoadNodeChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.RoadNodeChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.RoadNodeChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "WEGKNOOP_ALL.SHP",
                    new ZipArchiveShapeEntryTranslator(
                        encoding,
                        new RoadNodeChangeShapeRecordsTranslator()
                    )
                },
                {
                    "WEGSEGMENT_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                RoadSegmentChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<RoadSegmentChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new RoadSegmentChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "WEGSEGMENT_ALL.SHP",
                    new ZipArchiveShapeEntryTranslator(
                        encoding,
                        new RoadSegmentChangeShapeRecordsTranslator()
                    )
                },
                {
                    "ATTEUROPWEG_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                EuropeanRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<EuropeanRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new EuropeanRoadChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.EuropeanRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.EuropeanRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.EuropeanRoadChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "ATTNATIONWEG_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                NationalRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<NationalRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new NationalRoadChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.NationalRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.NationalRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.NationalRoadChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "ATTGENUMWEG_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                NumberedRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<NumberedRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new NumberedRoadChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.NumberedRoadChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.NumberedRoadChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.NumberedRoadChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "ATTRIJSTROKEN_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                RoadSegmentLaneChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<RoadSegmentLaneChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new RoadSegmentLaneChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentLaneChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentLaneChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentLaneChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "ATTWEGBREEDTE_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                RoadSegmentWidthChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<RoadSegmentWidthChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new RoadSegmentWidthChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentWidthChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentWidthChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentWidthChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "ATTWEGVERHARDING_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                RoadSegmentSurfaceChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<RoadSegmentSurfaceChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new RoadSegmentSurfaceChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentSurfaceChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.RoadSegmentSurfaceChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.RoadSegmentSurfaceChangeDbaseRecordsTranslator()
                                )
                            }
                        })
                },
                {
                    "RLTOGKRUISING_ALL.DBF", new ZipArchiveVersionedDbaseEntryTranslator(
                        encoding, new DbaseFileHeaderReadBehavior(true), new Dictionary<DbaseSchema, IZipArchiveEntryTranslator>
                        {
                            {
                                GradeSeparatedJunctionChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<GradeSeparatedJunctionChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new GradeSeparatedJunctionChangeDbaseRecordsTranslator()
                                )
                            },
                            {
                                Dbase.AfterFeatureCompare.V1.Schema.GradeSeparatedJunctionChangeDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.GradeSeparatedJunctionChangeDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.GradeSeparatedJunctionChangeDbaseRecordsTranslator()
                                )
                            }
                        })
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
                                Dbase.AfterFeatureCompare.V1.Schema.TransactionZoneDbaseRecord.Schema,
                                new ZipArchiveDbaseEntryTranslator<Dbase.AfterFeatureCompare.V1.Schema.TransactionZoneDbaseRecord>(
                                    encoding, new DbaseFileHeaderReadBehavior(true),
                                    new Dbase.AfterFeatureCompare.V1.Validation.TransactionZoneDbaseRecordsTranslator())
                            }
                        })
                }
            };
    }

    public TranslatedChanges Translate(ZipArchive archive)
    {
        if (archive == null)
        {
            throw new ArgumentNullException(nameof(archive));
        }

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
