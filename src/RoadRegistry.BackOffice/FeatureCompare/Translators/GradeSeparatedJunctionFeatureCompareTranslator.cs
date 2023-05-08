namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using RoadRegistry.BackOffice.Uploads;
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
        
        var wegsegmentenAdd = context.RoadSegments.Where(x => x.RecordType == RecordType.Added).ToList();
        var wegsegmentenIdentical = context.RoadSegments.Where(x => x.RecordType == RecordType.Identical).ToList();
        var wegsegmentenUpdate = context.RoadSegments.Where(x => x.RecordType == RecordType.Modified).ToList();

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

        foreach (var leveringFeature in leveringFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var boWegsegmentAddFeature = wegsegmentenAdd.Find(x => x.Id == leveringFeature.Attributes.BO_WS_OIDN);
            var onWegsegmentAddFeature = wegsegmentenAdd.Find(x => x.Id == leveringFeature.Attributes.ON_WS_OIDN);
            var boWegsegmentUpdateFeature = wegsegmentenUpdate.Find(x => x.CompareIdn == leveringFeature.Attributes.BO_WS_OIDN.ToString());
            var onWegsegmentUpdateFeature = wegsegmentenUpdate.Find(x => x.CompareIdn == leveringFeature.Attributes.ON_WS_OIDN.ToString());
            var boWegsegmentIdenticalFeature = wegsegmentenIdentical.Find(x => x.Id == leveringFeature.Attributes.BO_WS_OIDN);
            var onWegsegmentIdenticalFeature = wegsegmentenIdentical.Find(x => x.Id == leveringFeature.Attributes.ON_WS_OIDN);

            if (boWegsegmentUpdateFeature is not null && onWegsegmentAddFeature is not null)
            {
                var removeExtractFeatures = extractFeatures
                    .FindAll(x => x.Attributes.BO_WS_OIDN == boWegsegmentUpdateFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentAddFeature.Id);
                RemoveFeatures(removeExtractFeatures);

                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentAddFeature.EventIdn;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentAddFeature is not null && onWegsegmentUpdateFeature is not null)
            {
                var removeExtractFeatures = extractFeatures
                    .FindAll(x => x.Attributes.BO_WS_OIDN == boWegsegmentAddFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentUpdateFeature.Id);
                RemoveFeatures(removeExtractFeatures);

                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentAddFeature.EventIdn;
                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentAddFeature is not null && onWegsegmentAddFeature is not null)
            {
                var removeExtractFeatures = extractFeatures
                    .FindAll(x => x.Attributes.BO_WS_OIDN == boWegsegmentAddFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentAddFeature.Id);
                RemoveFeatures(removeExtractFeatures);

                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentAddFeature.EventIdn;
                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentAddFeature.EventIdn;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentUpdateFeature is not null && onWegsegmentIdenticalFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentUpdateFeature.Id
                              && x.Attributes.ON_WS_OIDN == onWegsegmentIdenticalFeature.Id);
                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentIdenticalFeature is not null && onWegsegmentUpdateFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentIdenticalFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentUpdateFeature.Id);

                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentAddFeature is not null && onWegsegmentIdenticalFeature is not null)
            {
                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentAddFeature.EventIdn;
                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentIdenticalFeature is not null && onWegsegmentAddFeature is not null)
            {
                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentAddFeature.EventIdn;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentUpdateFeature is not null && onWegsegmentUpdateFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentUpdateFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentUpdateFeature.Id);

                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentIdenticalFeature is not null && onWegsegmentIdenticalFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentIdenticalFeature.Id
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentIdenticalFeature.Id);

                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentUpdateFeature is not null && onWegsegmentAddFeature is null && onWegsegmentUpdateFeature is null && onWegsegmentIdenticalFeature is null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentUpdateFeature.Id
                                  && x.Attributes.ON_WS_OIDN == leveringFeature.Attributes.ON_WS_OIDN);

                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentAddFeature is not null && onWegsegmentAddFeature is null && onWegsegmentUpdateFeature is null && onWegsegmentIdenticalFeature is null)
            {
                var removeExtractFeatures = extractFeatures
                    .FindAll(x => x.Attributes.BO_WS_OIDN == boWegsegmentAddFeature.Id
                                  && x.Attributes.ON_WS_OIDN == leveringFeature.Attributes.ON_WS_OIDN);
                RemoveFeatures(removeExtractFeatures);

                leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentAddFeature.EventIdn;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentIdenticalFeature is not null && onWegsegmentAddFeature is null && onWegsegmentUpdateFeature is null && onWegsegmentIdenticalFeature is null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == boWegsegmentIdenticalFeature.Id
                                  && x.Attributes.ON_WS_OIDN == leveringFeature.Attributes.ON_WS_OIDN);
                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.BO_WS_OIDN = boWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentAddFeature is null && boWegsegmentUpdateFeature is null && boWegsegmentIdenticalFeature is null && onWegsegmentUpdateFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == leveringFeature.Attributes.BO_WS_OIDN
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentUpdateFeature.Id);
                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentUpdateFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }

            if (boWegsegmentAddFeature is null && boWegsegmentUpdateFeature is null && boWegsegmentIdenticalFeature is null && onWegsegmentAddFeature is not null)
            {
                var removeExtractFeatures = extractFeatures
                    .FindAll(x => x.Attributes.BO_WS_OIDN == leveringFeature.Attributes.BO_WS_OIDN
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentAddFeature.Id);
                RemoveFeatures(removeExtractFeatures);

                leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentAddFeature.EventIdn;
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }

            if (boWegsegmentAddFeature is null && boWegsegmentUpdateFeature is null && boWegsegmentIdenticalFeature is null && onWegsegmentIdenticalFeature is not null)
            {
                var hasExtractFeatures = extractFeatures
                    .Any(x => x.Attributes.BO_WS_OIDN == leveringFeature.Attributes.BO_WS_OIDN
                                  && x.Attributes.ON_WS_OIDN == onWegsegmentIdenticalFeature.Id);
                if (hasExtractFeatures)
                {
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
                }
                else
                {
                    leveringFeature.Attributes.ON_WS_OIDN = onWegsegmentIdenticalFeature.Id;
                    processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                }
            }
        }

        foreach (var extractFeature in extractFeatures)
        {
            var hasLeveringIdenticalFeatures = processedRecords
                .Any(x => x.RecordType.Equals(RecordType.Identical)
                          && x.Feature.Attributes.BO_WS_OIDN == extractFeature.Attributes.BO_WS_OIDN
                          && x.Feature.Attributes.ON_WS_OIDN == extractFeature.Attributes.ON_WS_OIDN);
            var hasExtractDeleteFeatures = processedRecords
                .Any(x => x.RecordType.Equals(RecordType.Removed)
                          && x.Feature.Attributes.BO_WS_OIDN == extractFeature.Attributes.BO_WS_OIDN
                          && x.Feature.Attributes.ON_WS_OIDN == extractFeature.Attributes.ON_WS_OIDN);

            if (!hasLeveringIdenticalFeatures && !hasExtractDeleteFeatures)
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

    private class ExtractsFeatureReader : FeatureReader<GradeSeparatedJunctionDbaseRecord, Feature>
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

    private class UploadsFeatureReader : FeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord, Feature>
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
