namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    internal class EuropeanRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<EuropeanRoadFeatureCompareAttributes>
    {
        public EuropeanRoadFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTEUROPWEG")
        {
        }

        protected override List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
        {
            var featureReader = new VersionedFeatureReader<Feature>(
                new ExtractsFeatureReader(Encoding),
                new UploadsFeatureReader(Encoding)
            );

            var dbfFileName = GetDbfFileName(featureType, fileName);

            return featureReader.Read(entries, dbfFileName);
        }

        protected override void HandleIdenticalRoadSegment(RoadSegmentRecord wegsegment, List<Feature> leveringFeatures, List<Feature> extractFeatures, List<Record> processedRecords)
        {
            var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN.ToString() == wegsegment.CompareIdn);
            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.WS_OIDN == wegsegment.Id);

            foreach (var leveringFeature in wegsegmentLeveringFeatures)
            {
                var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.WS_OIDN == leveringFeature.Attributes.WS_OIDN
                                                                   && x.Attributes.EUNUMMER == leveringFeature.Attributes.EUNUMMER);
                if (!leveringExtractFeatures.Any())
                {
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
                else
                {
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));

                    if (leveringExtractFeatures.Count > 1)
                    {
                        foreach (var extractFeature in leveringExtractFeatures.Skip(1))
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                        }
                    }
                }
            }

            foreach (var extractFeature in wegsegmentExtractFeatures)
            {
                var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN == extractFeature.Attributes.WS_OIDN
                                                                            && x.Attributes.EUNUMMER == extractFeature.Attributes.EUNUMMER);
                if (!extractLeveringFeatures.Any())
                {
                    processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                }
            }
        }

        protected override void HandleModifiedRoadSegment(RoadSegmentRecord wegsegment, List<Feature> leveringFeatures, List<Feature> extractFeatures, List<Record> processedRecords)
        {
            var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN.ToString() == wegsegment.CompareIdn);
            var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.WS_OIDN == wegsegment.Id);

            foreach (var leveringFeature in wegsegmentLeveringFeatures)
            {
                var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.WS_OIDN == wegsegment.Id
                                                                           && x.Attributes.EUNUMMER == leveringFeature.Attributes.EUNUMMER);
                if (!leveringExtractFeatures.Any())
                {
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
                }
                else
                {
                    processedRecords.Add(new Record(leveringExtractFeatures.First(), RecordType.Identical));
                }
            }

            foreach (var extractFeature in wegsegmentExtractFeatures)
            {
                {
                    var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN.ToString() == wegsegment.CompareIdn
                                                                           && x.Attributes.EUNUMMER == extractFeature.Attributes.EUNUMMER);

                    if (!extractLeveringFeatures.Any())
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                    }
                }
            }
        }

        protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
        {
            foreach (var record in records)
            {
                switch (record.RecordType.Translation.Identifier)
                {
                    case RecordType.AddedIdentifier:
                        changes = changes.AppendChange(
                            new AddRoadSegmentToEuropeanRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.EU_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                EuropeanRoadNumber.Parse(record.Feature.Attributes.EUNUMMER)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromEuropeanRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.EU_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                EuropeanRoadNumber.Parse(record.Feature.Attributes.EUNUMMER)
                            )
                        );
                        break;
                }
            }

            return changes;
        }

        private sealed class ExtractsFeatureReader : FeatureReader<RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new EuropeanRoadFeatureCompareAttributes
                {
                    EU_OIDN = dbaseRecord.EU_OIDN.Value,
                    EUNUMMER = dbaseRecord.EUNUMMER.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }

        private class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature>
        {
            public UploadsFeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new EuropeanRoadFeatureCompareAttributes
                {
                    EU_OIDN = dbaseRecord.EU_OIDN.Value,
                    EUNUMMER = dbaseRecord.EUNUMMER.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }
    }
}
