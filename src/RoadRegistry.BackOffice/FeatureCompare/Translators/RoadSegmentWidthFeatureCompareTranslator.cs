namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;

    internal class RoadSegmentWidthFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentWidthFeatureCompareAttributes>
    {
        public RoadSegmentWidthFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTWEGBREEDTE")
        {
        }

        protected override List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
        {
            var featureReader = new VersionedFeatureReader<Feature>(
                new ExtractsFeatureReader(Encoding),
                new UploadsV2FeatureReader(Encoding),
                new UploadsV1FeatureReader(Encoding)
            );

            var dbfFileName = GetDbfFileName(featureType, fileName);

            return featureReader.Read(entries, dbfFileName);
        }

        protected override bool Equals(Feature feature1, Feature feature2)
        {
            return feature1.Attributes.VANPOS == feature2.Attributes.VANPOS
                   && feature1.Attributes.TOTPOS == feature2.Attributes.TOTPOS
                   && feature1.Attributes.BREEDTE == feature2.Attributes.BREEDTE;
        }

        protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
        {
            foreach (var record in records)
            {
                var segmentId = new RoadSegmentId(record.Feature.Attributes.WS_OIDN);
                var width = new RoadSegmentWidthAttribute(
                    new AttributeId(record.Feature.Attributes.Id),
                    new RoadSegmentWidth(record.Feature.Attributes.BREEDTE),
                    RoadSegmentPosition.FromDouble(record.Feature.Attributes.VANPOS),
                    RoadSegmentPosition.FromDouble(record.Feature.Attributes.TOTPOS)
                );
                if (changes.TryFindRoadSegmentProvisionalChange(segmentId, out var provisionalChange))
                {
                    switch (provisionalChange)
                    {
                        case ModifyRoadSegment modifyRoadSegment:
                            switch (record.RecordType.Translation.Identifier)
                            {
                                case RecordType.IdenticalIdentifier:
                                    changes = changes.ReplaceProvisionalChange(modifyRoadSegment,
                                        modifyRoadSegment.WithWidth(width));
                                    break;
                                case RecordType.AddedIdentifier:
                                case RecordType.ModifiedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment,
                                        modifyRoadSegment.WithWidth(width));
                                    break;
                                case RecordType.RemovedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment);
                                    break;
                            }
                            break;
                    }
                }
                else if (changes.TryFindRoadSegmentChange(segmentId, out var change))
                {
                    switch (record.RecordType.Translation.Identifier)
                    {
                        case RecordType.IdenticalIdentifier:
                        case RecordType.AddedIdentifier:
                            switch (change)
                            {
                                case AddRoadSegment addRoadSegment:
                                    changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithWidth(width));
                                    break;
                                case ModifyRoadSegment modifyRoadSegment:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithWidth(width));
                                    break;
                            }
                            break;
                    }
                }
            }

            return changes;
        }

        private sealed class ExtractsFeatureReader : FeatureReader<RoadSegmentWidthAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentWidthAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
                {
                    Id = dbaseRecord.WB_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    BREEDTE = dbaseRecord.BREEDTE.Value
                });
            }
        }

        private sealed class UploadsV2FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord, Feature>
        {
            public UploadsV2FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
                {
                    Id = dbaseRecord.WB_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    BREEDTE = dbaseRecord.BREEDTE.Value
                });
            }
        }

        private sealed class UploadsV1FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord, Feature>
        {
            public UploadsV1FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
                {
                    Id = dbaseRecord.WB_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    BREEDTE = dbaseRecord.BREEDTE.Value
                });
            }
        }
    }
}
