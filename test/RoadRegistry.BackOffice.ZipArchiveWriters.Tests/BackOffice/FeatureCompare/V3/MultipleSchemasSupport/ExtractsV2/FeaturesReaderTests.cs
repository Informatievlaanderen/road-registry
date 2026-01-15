namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.MultipleSchemasSupport.ExtractsV2;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using GradeSeparatedJunction.Changes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.V3;
using RoadRegistry.Extracts.Schemas.ExtractV2.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadNodes;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V2;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Xunit.Sdk;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.V3.TranslatedChanges;

public class FeaturesReaderTests
{
    private readonly ILogger<ZipArchiveFeatureCompareTranslator> _logger;
    private readonly ITestOutputHelper _testOutputHelper;

    public FeaturesReaderTests(ILogger<ZipArchiveFeatureCompareTranslator> logger, ITestOutputHelper testOutputHelper)
    {
        _logger = logger;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task AllFeatureReadersCanRead()
    {
        var testData = new ExtractV2ZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var projectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        roadSegmentDbaseRecord1.WS_OIDN.Value = 1;
        roadSegmentDbaseRecord1.B_WK_OIDN.Value = 1;
        roadSegmentDbaseRecord1.E_WK_OIDN.Value = 2;
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        roadSegmentDbaseRecord2.WS_OIDN.Value = 2;
        roadSegmentDbaseRecord2.B_WK_OIDN.Value = 3;
        roadSegmentDbaseRecord2.E_WK_OIDN.Value = 4;
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, [roadSegmentDbaseRecord1]);
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, [roadSegmentDbaseRecord1, roadSegmentDbaseRecord2]);

        var roadSegmentLineString1 = fixture.Create<MultiLineString>();
        var roadSegmentLineString2 = fixture.CreateWhichIsDifferentThan((g1, g2) =>
            g1.RoadSegmentOverlapsWith(g2, 1.0), roadSegmentLineString1);
        var roadSegmentShapeContent1 = roadSegmentLineString1.ToShapeContent();
        var roadSegmentShapeContent2 = roadSegmentLineString2.ToShapeContent();

        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile([roadSegmentShapeContent1]);
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile([roadSegmentShapeContent1, roadSegmentShapeContent2]);

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, [europeanRoadDbaseRecord]);

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, [nationalRoadDbaseRecord]);

        var surfaceDbaseRecord1 = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord1.WV_OIDN.Value = 1;
        surfaceDbaseRecord1.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord1.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord1.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceDbaseRecord2 = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord2.WV_OIDN.Value = 2;
        surfaceDbaseRecord2.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        surfaceDbaseRecord2.VANPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Min;
        surfaceDbaseRecord2.TOTPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, [surfaceDbaseRecord1]);
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, [surfaceDbaseRecord1, surfaceDbaseRecord2]);

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, [roadNodeDbaseRecord1, roadNodeDbaseRecord2]);
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, [roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4]);

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile([roadNodeShapeContent1, roadNodeShapeContent2]);
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile([roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4]);

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value = 1;
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, [gradeSeparatedJunctionDbaseRecord]);

        var zipArchive = fixture.CreateUploadZipArchiveV2(
            testData,
            roadSegmentProjectionFormatStream: projectionFormatStream,
            roadNodeProjectionFormatStream: projectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            surfaceChangeStream: surfaceChangeStream,
            europeanRoadChangeStream: europeanRoadChangeStream,
            nationalRoadChangeStream: nationalRoadChangeStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var maxRoadSegmentId = Math.Max(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value);
        var roadSegment2TemporaryId = new RoadSegmentId(maxRoadSegmentId + 1);

        var expected = TranslatedChanges.Empty
            .AppendChange(new AddRoadNodeChange
            {
                TemporaryId = new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value),
                OriginalId = null,
                Geometry = GeometryTranslator.ToPoint(roadNodeShapeContent3.Shape).ToRoadNodeGeometry(),
                Type = RoadNodeType.ByIdentifier[roadNodeDbaseRecord3.TYPE.Value]
            })
            .AppendChange(new AddRoadNodeChange
            {
                TemporaryId = new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value),
                OriginalId = null,
                Geometry = GeometryTranslator.ToPoint(roadNodeShapeContent4.Shape).ToRoadNodeGeometry(),
                Type = RoadNodeType.ByIdentifier[roadNodeDbaseRecord4.TYPE.Value]
            })
            .AppendChange(new AddRoadSegmentChange
            {
                TemporaryId = roadSegment2TemporaryId,
                OriginalId = new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value),
                Geometry = GeometryTranslator.ToMultiLineString(roadSegmentShapeContent2.Shape).ToRoadSegmentGeometry(),
                StartNodeId = new RoadNodeId(roadSegmentDbaseRecord2.B_WK_OIDN.Value),
                EndNodeId = new RoadNodeId(roadSegmentDbaseRecord2.E_WK_OIDN.Value),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord2.METHODE.Value],
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(roadSegmentDbaseRecord2.BEHEERDER.Value!)),
                Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord2.MORFOLOGIE.Value]),
                Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord2.STATUS.Value]),
                Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord2.CATEGORIE.Value]),
                AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord2.TGBEP.Value]),
                SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>([(new(new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord2.VANPOS.Value)), new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord2.TOTPOS.Value))), RoadSegmentAttributeSide.Both, RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord2.TYPE.Value])]),
                StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(roadSegmentDbaseRecord2.LSTRNMID.Value), StreetNameLocalId.FromValue(roadSegmentDbaseRecord2.RSTRNMID.Value)),
                EuropeanRoadNumbers = [EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value!)],
                NationalRoadNumbers = [NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value!)]
            })
            .AppendChange(new AddGradeSeparatedJunctionChange
            {
                TemporaryId = new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                Type = GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value],
                UpperRoadSegmentId = new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                LowerRoadSegmentId = roadSegment2TemporaryId
            });

        using (zipArchive)
        {
            var sut = ZipArchiveFeatureCompareTranslatorV3Builder.Create();

            TranslatedChanges result = null;
            try
            {
                result = await sut.TranslateAsync(zipArchive, ZipArchiveMetadata.Empty, CancellationToken.None);

                Assert.Equal(expected, result);
            }
            catch (EqualException)
            {
                _testOutputHelper.WriteLine($"Expected:\n{expected.Describe()}");
                await File.WriteAllTextAsync("expected.txt", $"Expected:\n{expected.Describe()}");
                _testOutputHelper.WriteLine($"Actual:\n{result?.Describe()}");
                await File.WriteAllTextAsync("actual.txt", $"Actual:\n{result?.Describe()}");
                throw;
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var error in ex.Problems.OfType<FileError>())
                {
                    _testOutputHelper.WriteLine(error.Describe());
                }

                throw;
            }
        }
    }

    private static Fixture CreateFixture(ExtractV2ZipArchiveTestData testData)
    {
        var fixture = testData.Fixture;

        fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(composer => composer
            .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
            {
                EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
            })
            .OmitAutoProperties());

        fixture.Customize<GradeSeparatedJunctionDbaseRecord>(composer => composer
            .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
            {
                OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                TYPE =
                    { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
            })
            .OmitAutoProperties());

        fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(composer => composer
            .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
            {
                NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
            })
            .OmitAutoProperties());

        fixture.Customize<RoadNodeDbaseRecord>(composer => composer
            .FromFactory(_ => new RoadNodeDbaseRecord
            {
                TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
            })
            .OmitAutoProperties());

        fixture.Customize<RoadSegmentDbaseRecord>(composer => composer
            .FromFactory(random => new RoadSegmentDbaseRecord
            {
                WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                METHODE =
                {
                    Value = (short)fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                },
                BEHEERDER = { Value = fixture.Create<OrganizationId>() },
                MORFOLOGIE =
                    { Value = (short)fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                STATUS = { Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                CATEGORIE = { Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                LSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                RSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                TGBEP =
                {
                    Value = (short)fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                }
            })
            .OmitAutoProperties());

        fixture.Customize<RoadSegmentSurfaceAttributeDbaseRecord>(composer => composer
            .FromFactory(random => new RoadSegmentSurfaceAttributeDbaseRecord
            {
                WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
            })
            .OmitAutoProperties());

        return fixture;
    }

    private static RoadSegmentDynamicAttributeValues<StreetNameLocalId> BuildStreetNameIdAttributes(StreetNameLocalId? leftSideStreetNameId, StreetNameLocalId? rightSideStreetNameId)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(StreetNameLocalId.NotApplicable);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(leftSideStreetNameId.Value);
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(null, RoadSegmentAttributeSide.Left, leftSideStreetNameId ?? StreetNameLocalId.NotApplicable)
            .Add(null, RoadSegmentAttributeSide.Right, rightSideStreetNameId ?? StreetNameLocalId.NotApplicable);
    }
}
