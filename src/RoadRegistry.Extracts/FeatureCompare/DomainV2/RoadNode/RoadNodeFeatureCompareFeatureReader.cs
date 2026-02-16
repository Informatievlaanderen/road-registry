namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Dbase;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure.Messages;
using Schemas.Inwinning.RoadNodes;
using Point = NetTopologySuite.Geometries.Point;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadNodeFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadNodeFeatureCompareAttributes>>
{
    private readonly FileEncoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Wegknoop;

    public RoadNodeFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new InwinningFeatureReader(encoding))
    {
        _encoding = encoding;
    }

    public override (List<Feature<RoadNodeFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateProjectionFileLambert08(featureType, FileName, _encoding);
        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        switch (featureType)
        {
            case FeatureType.Change:
                AddToContext(features, featureType, context);
                break;
            case FeatureType.Extract:
                if (context.ZipArchiveMetadata.Inwinning)
                {
                    var excludeProblems = new List<Func<FileProblem, bool>>
                    {
                        p => p.Reason == nameof(DbaseFileProblems.RoadNodeGrensknoopMismatch) && p.GetParameterValue("Actual") == "-8"
                    };
                    problems = ZipArchiveProblems.None + problems
                        .Where(p => excludeProblems.All(x => !x(p)));
                }
                break;
            case FeatureType.Integration:
                problems = ZipArchiveProblems.None + problems
                    .GetMissingOrInvalidFileProblems()
                    .Where(x => !x.File.Equals(featureType.ToProjectionFileName(FileName), StringComparison.InvariantCultureIgnoreCase));

                foreach (var feature in features)
                {
                    if (context.ChangedRoadNodes.TryGetValue(feature.Attributes.Id, out var knownRoadNode))
                    {
                        var recordContext = FileName.AtDbaseRecord(featureType, feature.RecordNumber);
                        problems += recordContext.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange(feature.Attributes.Id, knownRoadNode.RecordNumber);
                    }
                }
                break;
        }

        return (features, problems);
    }

    private void AddToContext(List<Feature<RoadNodeFeatureCompareAttributes>> features, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        if (featureType != FeatureType.Change)
        {
            throw new NotSupportedException($"Only {FeatureType.Change} features can be added to the context");
        }

        foreach (var feature in features)
        {
            if (context.ChangedRoadNodes.ContainsKey(feature.Attributes.Id))
            {
                continue;
            }

            context.ChangedRoadNodes.Add(feature.Attributes.Id, feature);
        }
    }

    private sealed class InwinningFeatureReader : ZipArchiveShapeFeatureReader<RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public InwinningFeatureReader(Encoding encoding)
            : base(encoding, RoadNodeFeatureCompareFeatureReader.FileName, RoadNodeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadNodeDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return new RecordData
            {
                Geometry = geometry,
                WK_OIDN = dbaseRecord.WK_OIDN.GetValue(),
                GRENSKNOOP = dbaseRecord.GRENSKNOOP.GetValue(),
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record RecordData
    {
        public required Geometry Geometry { get; init; }
        public required int? WK_OIDN { get; init; }
        public required int? GRENSKNOOP { get; init; }

        public (Feature<RoadNodeFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(WK_OIDN), WK_OIDN);

            var problems = ZipArchiveProblems.None;

            Point? ReadGeometry()
            {
                if (Geometry is Point point)
                {
                    return point;
                }

                var recordContext = fileName
                    .AtShapeRecord(featureType, recordNumber);

                problems += recordContext.ShapeRecordShapeGeometryTypeMismatch(ShapeType.Point, Geometry.GeometryType);
                return null;
            }

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

            bool? ReadGrensknoop()
            {
                if (GRENSKNOOP is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(GRENSKNOOP));
                }
                else
                {
                    var boolValue = GRENSKNOOP.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadNodeGrensknoopMismatch(GRENSKNOOP.Value);
                }

                return null;
            }

            var feature = Feature.New(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                Geometry = ReadGeometry(),
                Id = ReadId(),
                Grensknoop = ReadGrensknoop(),
            });
            return (feature, problems);
        }
    }
}
