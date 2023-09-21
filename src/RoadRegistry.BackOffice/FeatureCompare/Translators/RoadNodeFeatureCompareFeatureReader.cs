namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadNodes;
using NetTopologySuite.IO;
using ShapeFile;
using Uploads;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadNodeFeatureCompareAttributes>>
{
    private readonly Encoding _encoding;

    public RoadNodeFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
        _encoding = encoding;
    }

    public override (List<Feature<RoadNodeFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, fileName, context);

        problems += archive.ValidateProjectionFile(featureType, fileName, _encoding);
        problems += ReadShapeFile(features, archive, featureType, fileName);
        problems += archive.ValidateUniqueIdentifiers(features, featureType, fileName, feature => feature.Attributes.Id);

        if (featureType == FeatureType.Change)
        {
            AddToContext(features, featureType, context);
        }

        return (features, problems);
    }

    private ZipArchiveProblems ReadShapeFile(List<Feature<RoadNodeFeatureCompareAttributes>> features, ZipArchive archive, FeatureType featureType, ExtractFileName fileName)
    {
        var problems = ZipArchiveProblems.None;

        var shpFileName = featureType.GetShapeFileName(fileName);
        var shpEntry = archive.FindEntry(shpFileName);
        if (shpEntry is null)
        {
            problems += problems.RequiredFileMissing(shpFileName);
        }
        else
        {
            var dbfFileName = featureType.GetDbaseFileName(fileName);
            var dbfEntry = archive.FindEntry(dbfFileName);
            if (dbfEntry is not null)
            {
                var shpReader = new ZipArchiveShapeFileReader();
                RecordNumber? currentRecordNumber = null;

                try
                {
                    foreach (var (geometry, recordNumber) in shpReader.Read(shpEntry))
                    {
                        var recordContext = shpEntry.AtShapeRecord(recordNumber);
                        currentRecordNumber = recordNumber;

                        var index = features.FindIndex(x => x.RecordNumber.Equals(recordNumber));
                        if (index == -1)
                        {
                            problems += recordContext.DbaseRecordMissing();
                            continue;
                        }

                        if (geometry is Point point)
                        {
                            features[index] = features[index] with
                            {
                                Attributes = features[index].Attributes with
                                {
                                    Geometry = point
                                }
                            };
                        }
                        else
                        {
                            problems += recordContext.ShapeRecordShapeGeometryTypeMismatch(ShapeGeometryType.Point, geometry.GeometryType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    problems += shpEntry
                        .AtShapeRecord(currentRecordNumber ?? RecordNumber.Initial)
                        .HasShapeRecordFormatError(ex);
                }

                var featuresWithoutGeometry = features.Where(x => x.Attributes.Geometry is null).ToArray();
                if (featuresWithoutGeometry.Any())
                {
                    foreach (var feature in featuresWithoutGeometry)
                    {
                        var recordContext = dbfEntry.AtDbaseRecord(feature.RecordNumber);
                        problems += recordContext.RoadNodeGeometryMissing(feature.Attributes.Id);
                    }
                }

                if (currentRecordNumber is null)
                {
                    problems += shpEntry.HasNoShapeRecords();
                }
            }
        }

        return problems;
    }

    private void AddToContext(List<Feature<RoadNodeFeatureCompareAttributes>> features, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        if (featureType != FeatureType.Change)
        {
            throw new NotSupportedException($"Only {FeatureType.Change} features can be added to the context");
        }

        foreach (var feature in features)
        {
            if (context.KnownRoadNodes.ContainsKey(feature.Attributes.Id))
            {
                continue;
            }

            context.KnownRoadNodes.Add(feature.Attributes.Id, feature);
        }
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadNodeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, RoadNodeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WK_OIDN = dbaseRecord.WK_OIDN.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WK_OIDN = dbaseRecord.WK_OIDN.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV1FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WK_OIDN = dbaseRecord.WK_OIDN.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? WK_OIDN { get; init; }
        public int? TYPE { get; init; }

        public (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName.AtDbaseRecord(featureType, recordNumber);

            var problems = ZipArchiveProblems.None;

            RoadNodeId ReadId()
            {
                if (WK_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WK_OIDN));
                }
                else if (RoadNodeId.Accepts(WK_OIDN.Value))
                {
                    return new RoadNodeId(WK_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.RoadNodeIdOutOfRange(WK_OIDN.Value);
                }

                return default;
            }

            RoadNodeType ReadType()
            {
                if (TYPE is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TYPE));
                }
                else if (RoadNodeType.ByIdentifier.TryGetValue(TYPE.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadNodeTypeMismatch(TYPE.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                Id = ReadId(),
                Type = ReadType()
            });
            return (feature, problems);
        }
    }
}
