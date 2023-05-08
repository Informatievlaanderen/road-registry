namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    internal class NationalRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NationalRoadFeatureCompareAttributes>
    {
        public NationalRoadFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTNATIONWEG")
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
                                                                   && x.Attributes.IDENT2 == leveringFeature.Attributes.IDENT2);
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
                var extractLeveringFeature = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN == extractFeature.Attributes.WS_OIDN
                                                                       && x.Attributes.IDENT2 == extractFeature.Attributes.IDENT2);
                if (!extractLeveringFeature.Any())
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
                                                                           && x.Attributes.IDENT2 == leveringFeature.Attributes.IDENT2);
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
                var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.WS_OIDN == wegsegment.Id
                                                                            && x.Attributes.IDENT2 == extractFeature.Attributes.IDENT2);
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
                            new AddRoadSegmentToNationalRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.NW_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                NationalRoadNumber.Parse(record.Feature.Attributes.IDENT2)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromNationalRoad(
                                record.Feature.RecordNumber,
                                new AttributeId(record.Feature.Attributes.NW_OIDN),
                                new RoadSegmentId(record.Feature.Attributes.WS_OIDN),
                                NationalRoadNumber.Parse(record.Feature.Attributes.IDENT2)
                            )
                        );
                        break;
                }
            }

            return changes;
        }

        private sealed class ExtractsFeatureReader : FeatureReader<RoadSegmentNationalRoadAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new NationalRoadFeatureCompareAttributes
                {
                    IDENT2 = dbaseRecord.IDENT2.Value,
                    NW_OIDN = dbaseRecord.NW_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }

        private class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord, Feature>
        {
            public UploadsFeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new NationalRoadFeatureCompareAttributes
                {
                    IDENT2 = dbaseRecord.IDENT2.Value,
                    NW_OIDN = dbaseRecord.NW_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value
                });
            }
        }
    }
}
