namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;

    internal class RoadSegmentLaneFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentLaneFeatureCompareAttributes>
    {
        public RoadSegmentLaneFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTRIJSTROKEN")
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
                   && feature1.Attributes.AANTAL == feature2.Attributes.AANTAL
                   && feature1.Attributes.RICHTING == feature2.Attributes.RICHTING;
        }

        protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
        {
            foreach (var record in records)
            {
                var segmentId = new RoadSegmentId(record.Feature.Attributes.WS_OIDN);
                var lane = new RoadSegmentLaneAttribute(
                    new AttributeId(record.Feature.Attributes.Id),
                    new RoadSegmentLaneCount(record.Feature.Attributes.AANTAL),
                    RoadSegmentLaneDirection.ByIdentifier[record.Feature.Attributes.RICHTING],
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
                                        modifyRoadSegment.WithLane(lane));
                                    break;
                                case RecordType.AddedIdentifier:
                                case RecordType.ModifiedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment,
                                        modifyRoadSegment.WithLane(lane));
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
                                    changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithLane(lane));
                                    break;
                                case ModifyRoadSegment modifyRoadSegment:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithLane(lane));
                                    break;
                            }
                            break;
                    }
                }
            }

            return changes;
        }

        private class ExtractsFeatureReader : FeatureReader<RoadSegmentLaneAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentLaneAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
                {
                    Id = dbaseRecord.RS_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    AANTAL = dbaseRecord.AANTAL.Value,
                    RICHTING = dbaseRecord.RICHTING.Value
                });
            }
        }

        private class UploadsV2FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord, Feature>
        {
            public UploadsV2FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
                {
                    Id = dbaseRecord.RS_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    AANTAL = dbaseRecord.AANTAL.Value,
                    RICHTING = dbaseRecord.RICHTING.Value
                });
            }
        }

        private class UploadsV1FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord, Feature>
        {
            public UploadsV1FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
                {
                    Id = dbaseRecord.RS_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    AANTAL = dbaseRecord.AANTAL.Value,
                    RICHTING = dbaseRecord.RICHTING.Value
                });
            }
        }
    }
}
