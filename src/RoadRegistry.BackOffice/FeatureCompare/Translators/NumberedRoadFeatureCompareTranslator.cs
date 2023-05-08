namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    internal class NumberedRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NumberedRoadFeatureCompareAttributes>
    {
        public NumberedRoadFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTGENUMWEG")
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
                                                                   && x.Attributes.IDENT8 == leveringFeature.Attributes.IDENT8);
                if (!leveringExtractFeatures.Any())
                {
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
                else
                {
                    for (var i = 0; i < leveringExtractFeatures.Count; i++)
                    {
                        var extractFeature = leveringExtractFeatures[i];

                        if (i == 0)
                        {
                            if (leveringFeature.Attributes.VOLGNUMMER != extractFeature.Attributes.VOLGNUMMER ||
                                leveringFeature.Attributes.RICHTING != extractFeature.Attributes.RICHTING)
                            {
                                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                            }
                            else
                            {
                                processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                            }
                        }
                        else
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                        }
                    }
                }
            }

            foreach (var extractFeature in wegsegmentExtractFeatures)
            {
                var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN == extractFeature.Attributes.WS_OIDN
                                                                            && x.Attributes.IDENT8 == extractFeature.Attributes.IDENT8);
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
                var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.WS_OIDN == wegsegment.Id && x.Attributes.IDENT8 == leveringFeature.Attributes.IDENT8);
                if (!leveringExtractFeatures.Any())
                {
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
                }
                else
                {
                    for (var i = 0; i < leveringExtractFeatures.Count; i++)
                    {
                        var extractFeature = leveringExtractFeatures[i];

                        if (i == 0)
                        {
                            if (leveringFeature.Attributes.VOLGNUMMER != extractFeature.Attributes.VOLGNUMMER ||
                                leveringFeature.Attributes.RICHTING != extractFeature.Attributes.RICHTING)
                            {
                                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                                processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
                            }
                            else
                            {
                                processedRecords.Add(new Record(extractFeature, RecordType.Identical));
                            }
                        }
                        else
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                        }
                    }
                }
            }

            foreach (var extractFeature in wegsegmentExtractFeatures)
            {
                var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN.ToString() == wegsegment.CompareIdn
                                                                       && x.Attributes.IDENT8 == extractFeature.Attributes.IDENT8);
                if (!extractLeveringFeatures.Any())
                {
                    processedRecords.Add(new Record(extractFeature, RecordType.Removed));
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
                            new AddRoadSegmentToNumberedRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.GW_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                NumberedRoadNumber.Parse(record.Feature.Attributes.IDENT8),
                                RoadSegmentNumberedRoadDirection.ByIdentifier[record.Feature.Attributes.RICHTING],
                                new RoadSegmentNumberedRoadOrdinal(record.Feature.Attributes.VOLGNUMMER)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromNumberedRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.GW_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                NumberedRoadNumber.Parse(record.Feature.Attributes.IDENT8)
                            )
                        );
                        break;
                }
            }

            return changes;
        }

        private sealed class ExtractsFeatureReader : FeatureReader<RoadSegmentNumberedRoadAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new NumberedRoadFeatureCompareAttributes
                {
                    GW_OIDN = dbaseRecord.GW_OIDN.Value,
                    IDENT8 = dbaseRecord.IDENT8.Value,
                    RICHTING = dbaseRecord.RICHTING.Value,
                    VOLGNUMMER = dbaseRecord.VOLGNUMMER.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }

        private sealed class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord, Feature>
        {
            public UploadsFeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new NumberedRoadFeatureCompareAttributes
                {
                    GW_OIDN = dbaseRecord.GW_OIDN.Value,
                    IDENT8 = dbaseRecord.IDENT8.Value,
                    RICHTING = dbaseRecord.RICHTING.Value,
                    VOLGNUMMER = dbaseRecord.VOLGNUMMER.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }
    }
}
