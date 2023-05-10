namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using RoadRegistry.BackOffice.Uploads;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class GradeSeparatedJunctionFeatureCompareTranslator : FeatureCompareTranslatorBase<GradeSeparatedJunctionFeatureCompareAttributes>
{
    private record Record(Feature Feature, RecordType RecordType);

    public GradeSeparatedJunctionFeatureCompareTranslator(Encoding encoding)
        : base(encoding)
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

    public override Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken)
    {
        var entries = context.Entries;

        var (extractFeatures, leveringFeatures) = ReadExtractAndLeveringFeatures(entries, "RLTOGKRUISING");
        
        var processedRecords = new List<Record>();

        void RemoveFeatures(ICollection<Feature> features)
        {
            foreach (var feature in features)
            {
                if (!processedRecords.Any(x => x.Feature.Attributes.OK_OIDN == feature.Attributes.OK_OIDN
                    && x.RecordType.Equals(RecordType.Removed)))
                {
                    processedRecords.Add(new Record(feature, RecordType.Removed));
                }
            }
        }

        var recordTypes = new[] { RecordType.Added, RecordType.Modified, RecordType.Identical, RecordType.Removed };

        foreach (var leveringFeature in leveringFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var boRecordType in recordTypes)
            {
                foreach (var onRecordType in recordTypes)
                {
                    var boWegsegmentFeature = context.RoadSegments.SingleOrDefault(x => x.RecordType.Equals(boRecordType) && (x.Id == leveringFeature.Attributes.BO_WS_OIDN || x.TempId == leveringFeature.Attributes.BO_WS_OIDN));
                    if (boWegsegmentFeature is null)
                    {
                        continue;
                    }

                    var onWegsegmentFeature = context.RoadSegments.SingleOrDefault(x => x.RecordType.Equals(onRecordType) && (x.Id == leveringFeature.Attributes.ON_WS_OIDN || x.TempId == leveringFeature.Attributes.ON_WS_OIDN));
                    if (onWegsegmentFeature is null)
                    {
                        continue;
                    }

                    var boWegsegmentIdOld = boWegsegmentFeature.Id;
                    var boWegsegmentIdNew = boWegsegmentFeature.GetActualId();
                    var onWegsegmentIdOld = onWegsegmentFeature.Id;
                    var onWegsegmentIdNew = onWegsegmentFeature.GetActualId();

                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdNew;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdNew;

                    if (boWegsegmentFeature.RecordType.Equals(RecordType.Added) || onWegsegmentFeature.RecordType.Equals(RecordType.Added))
                    {
                        var boOrOnSegmentIsIdentical = boWegsegmentFeature.RecordType.Equals(RecordType.Identical) || onWegsegmentFeature.RecordType.Equals(RecordType.Identical);
                        if (!boOrOnSegmentIsIdentical)
                        {
                            var removeExtractFeatures = extractFeatures
                                .FindAll(x => x.Attributes.BO_WS_OIDN == boWegsegmentIdOld
                                              && x.Attributes.ON_WS_OIDN == onWegsegmentIdOld);
                            RemoveFeatures(removeExtractFeatures);
                        }

                        processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                    }
                    else
                    {
                        var hasExtractFeatures = extractFeatures
                            .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentIdOld
                                      && x.Attributes.ON_WS_OIDN == onWegsegmentIdOld
                                      && x.Attributes.TYPE == leveringFeature.Attributes.TYPE);

                        processedRecords.Add(new Record(leveringFeature, hasExtractFeatures ? RecordType.Identical : RecordType.Added));
                    }
                }
            }
        }

        foreach (var extractFeature in extractFeatures)
        {
            var hasLeveringIdenticalOrRemovedFeatures = processedRecords
                .Any(x => (x.RecordType.Equals(RecordType.Identical) || x.RecordType.Equals(RecordType.Removed))
                          && x.Feature.Attributes.BO_WS_OIDN == extractFeature.Attributes.BO_WS_OIDN
                          && x.Feature.Attributes.ON_WS_OIDN == extractFeature.Attributes.ON_WS_OIDN);

            if (!hasLeveringIdenticalOrRemovedFeatures)
            {
                RemoveFeatures(new[] { extractFeature });
            }
        }

        {
            var notProcessedExtractFeatures = extractFeatures.FindAll(extractFeature =>
                processedRecords.All(processedRecord => processedRecord.Feature.Attributes.OK_OIDN != extractFeature.Attributes.OK_OIDN)
            );

            RemoveFeatures(notProcessedExtractFeatures);
        }

        foreach (var record in processedRecords)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            new GradeSeparatedJunctionId(record.Feature.Attributes.OK_OIDN),
                            GradeSeparatedJunctionType.ByIdentifier[record.Feature.Attributes.TYPE],
                            new RoadSegmentId(record.Feature.Attributes.BO_WS_OIDN),
                            new RoadSegmentId(record.Feature.Attributes.ON_WS_OIDN)
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveGradeSeparatedJunction(
                            record.Feature.RecordNumber,
                            new GradeSeparatedJunctionId(record.Feature.Attributes.OK_OIDN)
                        )
                    );
                    break;
            }
        }

        return Task.FromResult(changes);
    }

    private sealed class ExtractsFeatureReader : FeatureReader<GradeSeparatedJunctionDbaseRecord, Feature>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, GradeSeparatedJunctionDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                BO_WS_OIDN = dbaseRecord.BO_WS_OIDN.Value,
                OK_OIDN = dbaseRecord.OK_OIDN.Value,
                ON_WS_OIDN = dbaseRecord.ON_WS_OIDN.Value,
                TYPE = dbaseRecord.TYPE.Value
            });
        }
    }

    private sealed class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord, Feature>
    {
        public UploadsFeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override Feature ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord dbaseRecord)
        {
            return new Feature(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                BO_WS_OIDN = dbaseRecord.BO_WS_OIDN.Value,
                OK_OIDN = dbaseRecord.OK_OIDN.Value,
                ON_WS_OIDN = dbaseRecord.ON_WS_OIDN.Value,
                TYPE = dbaseRecord.TYPE.Value
            });
        }
    }
}
