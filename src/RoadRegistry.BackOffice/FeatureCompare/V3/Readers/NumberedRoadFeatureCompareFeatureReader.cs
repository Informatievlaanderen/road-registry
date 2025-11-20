namespace RoadRegistry.BackOffice.FeatureCompare.V3.Readers;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Models;
using RoadSegment.ValueObjects;
using Translators;
using Uploads;
using Validation;

public class NumberedRoadFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<NumberedRoadFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.AttGenumweg;

    public NumberedRoadFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding))
    {
    }

    public override (List<Feature<NumberedRoadFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        return (features, problems);
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentNumberedRoadAttributeDbaseRecord, Feature<NumberedRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, NumberedRoadFeatureCompareFeatureReader.FileName, RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<NumberedRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                GW_OIDN = dbaseRecord.GW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT8 = dbaseRecord.IDENT8.GetValue(),
                RICHTING = dbaseRecord.RICHTING.GetValue(),
                VOLGNUMMER = dbaseRecord.VOLGNUMMER.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord, Feature<NumberedRoadFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, NumberedRoadFeatureCompareFeatureReader.FileName, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<NumberedRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                GW_OIDN = dbaseRecord.GW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT8 = dbaseRecord.IDENT8.GetValue(),
                RICHTING = dbaseRecord.RICHTING.GetValue(),
                VOLGNUMMER = dbaseRecord.VOLGNUMMER.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? GW_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public string IDENT8 { get; init; }
        public int? RICHTING { get; init; }
        public int? VOLGNUMMER { get; init; }

        public (Feature<NumberedRoadFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(GW_OIDN), GW_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (GW_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(GW_OIDN));
                }
                else if (AttributeId.Accepts(GW_OIDN.Value))
                {
                    return new AttributeId(GW_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.IdentifierZero();
                }

                return default;
            }

            RoadSegmentId ReadRoadSegmentId()
            {
                if (WS_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WS_OIDN));
                }
                else if (RoadSegmentId.Accepts(WS_OIDN.Value))
                {
                    return new RoadSegmentId(WS_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentIdOutOfRange(WS_OIDN.Value);
                }

                return default;
            }

            NumberedRoadNumber ReadNumber()
            {
                if (IDENT8 is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(IDENT8));
                }
                else if (NumberedRoadNumber.TryParse(IDENT8, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.NotNumberedRoadNumber(IDENT8);
                }

                return default;
            }

            RoadSegmentNumberedRoadDirection ReadDirection()
            {
                if (RICHTING is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(RICHTING));
                }
                else if (RoadSegmentNumberedRoadDirection.ByIdentifier.TryGetValue(RICHTING.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.NumberedRoadDirectionMismatch(RICHTING.Value);
                }

                return default;
            }

            RoadSegmentNumberedRoadOrdinal ReadOrdinal()
            {
                if (VOLGNUMMER is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(VOLGNUMMER));
                }
                else if (RoadSegmentNumberedRoadOrdinal.Accepts(VOLGNUMMER.Value))
                {
                    return new RoadSegmentNumberedRoadOrdinal(VOLGNUMMER.Value);
                }
                else
                {
                    problems += problemBuilder.NumberedRoadOrdinalOutOfRange(VOLGNUMMER.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new NumberedRoadFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                Number = ReadNumber(),
                Direction = ReadDirection(),
                Ordinal = ReadOrdinal()
            });
            return (feature, problems);
        }
    }
}
