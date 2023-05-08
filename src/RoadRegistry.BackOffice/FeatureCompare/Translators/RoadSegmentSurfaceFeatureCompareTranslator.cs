namespace RoadRegistry.BackOffice.FeatureCompare.Translators
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Uploads;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Text;

    internal class RoadSegmentSurfaceFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentSurfaceFeatureCompareAttributes>
    {
        public RoadSegmentSurfaceFeatureCompareTranslator(Encoding encoding)
            : base(encoding, "ATTWEGVERHARDING")
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
                   && feature1.Attributes.TYPE == feature2.Attributes.TYPE;
        }

        protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
        {
            foreach (var record in records)
            {
                var segmentId = new RoadSegmentId(record.Feature.Attributes.WS_OIDN);
                var surface = new RoadSegmentSurfaceAttribute(
                    new AttributeId(record.Feature.Attributes.Id),
                    RoadSegmentSurfaceType.ByIdentifier[record.Feature.Attributes.TYPE],
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
                                        modifyRoadSegment.WithSurface(surface));
                                    break;
                                case RecordType.AddedIdentifier:
                                case RecordType.ModifiedIdentifier:
                                    changes = changes.ReplaceChange(modifyRoadSegment,
                                        modifyRoadSegment.WithSurface(surface));
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
                                    changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithSurface(surface));
                                    break;
                                case ModifyRoadSegment modifyRoadSegment:
                                    changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithSurface(surface));
                                    break;
                            }
                            break;
                    }
                }
            }

            return changes;
        }

        private class ExtractsFeatureReader : FeatureReader<RoadSegmentSurfaceAttributeDbaseRecord, Feature>
        {
            public ExtractsFeatureReader(Encoding encoding)
                : base(encoding, RoadSegmentSurfaceAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
                {
                    Id = dbaseRecord.WV_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    TYPE = dbaseRecord.TYPE.Value
                });
            }
        }

        private class UploadsV2FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature>
        {
            public UploadsV2FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
                {
                    Id = dbaseRecord.WV_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    TYPE = dbaseRecord.TYPE.Value
                });
            }
        }

        private class UploadsV1FeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature>
        {
            public UploadsV1FeatureReader(Encoding encoding)
                : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
            {
            }

            protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
            {
                return new Feature(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
                {
                    Id = dbaseRecord.WV_OIDN.Value,
                    WS_OIDN = dbaseRecord.WS_OIDN.Value,
                    VANPOS = dbaseRecord.VANPOS.Value,
                    TOTPOS = dbaseRecord.TOTPOS.Value,
                    TYPE = dbaseRecord.TYPE.Value
                });
            }
        }
    }
}
