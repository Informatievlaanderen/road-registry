 namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.MultipleSchemasSupport.Inwinning;

 using AutoFixture;
 using Be.Vlaanderen.Basisregisters.EventHandling;
 using Be.Vlaanderen.Basisregisters.Shaperon;
 using NetTopologySuite.Geometries;
 using Newtonsoft.Json;
 using RoadRegistry.Extensions;
 using RoadRegistry.Extracts.Infrastructure.Dbase;
 using RoadRegistry.Extracts.Schemas.Inwinning;
 using RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;
 using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
 using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
 using RoadRegistry.Extracts.Uploads;
 using RoadRegistry.GradeSeparatedJunction.Changes;
 using RoadRegistry.RoadNode.Changes;
 using RoadRegistry.RoadSegment.Changes;
 using RoadRegistry.RoadSegment.ValueObjects;
 using RoadRegistry.Tests.BackOffice;
 using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
 using Xunit.Abstractions;
 using Xunit.Sdk;
 using Polygon = NetTopologySuite.Geometries.Polygon;
 using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

 public class FeaturesReaderTests
 {
     private readonly ITestOutputHelper _testOutputHelper;

     public FeaturesReaderTests(ITestOutputHelper testOutputHelper)
     {
         _testOutputHelper = testOutputHelper;
     }

     [Fact]
     public async Task AllFeatureReadersCanRead()
     {
         var testData = new DomainV2ZipArchiveTestData();
         var fixture = CreateFixture(testData);

         var projectionFormatStream = fixture.CreateLambert08ProjectionFormatFileWithOneRecord();

         var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
         roadSegmentDbaseRecord1.WS_TEMPID.Value = 1;
         var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
         roadSegmentDbaseRecord2.WS_TEMPID.Value = 2;
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
         europeanRoadDbaseRecord.WS_TEMPID.Value = roadSegmentDbaseRecord2.WS_TEMPID.Value;
         var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, [europeanRoadDbaseRecord]);

         var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
         nationalRoadDbaseRecord.WS_TEMPID.Value = roadSegmentDbaseRecord2.WS_TEMPID.Value;
         var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, [nationalRoadDbaseRecord]);

         var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
         var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
         var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
         var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
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
         gradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = roadSegmentDbaseRecord1.WS_TEMPID.Value;
         gradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = roadSegmentDbaseRecord2.WS_TEMPID.Value;
         var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, [gradeSeparatedJunctionDbaseRecord]);

         var transactionZonePolygon = fixture.Create<Polygon>();
         var transactionZoneShapeContent = transactionZonePolygon.ToShapeContent();
         var transactionZoneShapeStream = fixture.CreateTransactionZoneShapeFile([transactionZoneShapeContent]);

         var zipArchive = fixture.CreateUploadZipArchiveV2(
             testData,
             roadSegmentProjectionFormatStream: projectionFormatStream,
             roadNodeProjectionFormatStream: projectionFormatStream,
             transactionZoneProjectionFormatStream: projectionFormatStream,
             roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
             roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
             roadNodeShapeExtractStream: roadNodeShapeExtractStream,
             roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
             roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
             roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
             roadNodeShapeChangeStream: roadNodeShapeChangeStream,
             roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
             europeanRoadChangeStream: europeanRoadChangeStream,
             nationalRoadChangeStream: nationalRoadChangeStream,
             gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream,
             transactionZoneStream: fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema),
             transactionZoneShapeStream: transactionZoneShapeStream
         );

         var maxRoadSegmentId = Math.Max(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value);
         var roadSegment2TemporaryId = new RoadSegmentId(maxRoadSegmentId + 1);

         var segment2Geometry = GeometryTranslator.ToMultiLineStringLambert08(roadSegmentShapeContent2.Shape).ToRoadSegmentGeometry();

         var expected = TranslatedChanges.Empty
             .AppendChange(new AddRoadNodeChange
             {
                 TemporaryId = new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value),
                 OriginalId = null,
                 Geometry = GeometryTranslator.ToPointLambert08(roadNodeShapeContent3.Shape).ToRoadNodeGeometry(),
                 Grensknoop = false
             })
             .AppendChange(new AddRoadNodeChange
             {
                 TemporaryId = new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value),
                 OriginalId = null,
                 Geometry = GeometryTranslator.ToPointLambert08(roadNodeShapeContent4.Shape).ToRoadNodeGeometry(),
                 Grensknoop = false
             })
             .AppendChange(new AddRoadSegmentChange
             {
                 TemporaryId = roadSegment2TemporaryId,
                 OriginalId = new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value),
                 Geometry = segment2Geometry,
                 GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                 Status = RoadSegmentStatusV2.ByIdentifier[roadSegmentDbaseRecord2.STATUS.Value],
                 MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>([
                    (new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(segment2Geometry.Value.Length)), RoadSegmentAttributeSide.Left, new OrganizationId(roadSegmentDbaseRecord2.LBEHEER.Value!)),
                    (new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(segment2Geometry.Value.Length)), RoadSegmentAttributeSide.Right, new OrganizationId(roadSegmentDbaseRecord2.RBEHEER.Value!))
                 ]),
                 Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(RoadSegmentMorphologyV2.ByIdentifier[roadSegmentDbaseRecord2.MORF.Value], segment2Geometry),
                 Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.ByIdentifier[roadSegmentDbaseRecord2.WEGCAT.Value], segment2Geometry),
                 AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(RoadSegmentAccessRestrictionV2.ByIdentifier[roadSegmentDbaseRecord2.TOEGANG.Value], segment2Geometry),
                 SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(RoadSegmentSurfaceTypeV2.ByIdentifier[roadSegmentDbaseRecord2.VERHARDING.Value], segment2Geometry),
                 StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>([
                     (new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(segment2Geometry.Value.Length)), RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(roadSegmentDbaseRecord2.LSTRNMID.Value ?? StreetNameLocalId.NotApplicable)!.Value),
                     (new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(segment2Geometry.Value.Length)), RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(roadSegmentDbaseRecord2.RSTRNMID.Value ?? StreetNameLocalId.NotApplicable)!.Value)
                 ]),CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentDbaseRecord2.AUTOHEEN.Value.ToBooleanFromDbaseValue() ?? false, segment2Geometry),
                 CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentDbaseRecord2.AUTOTERUG.Value.ToBooleanFromDbaseValue() ?? false, segment2Geometry),
                 BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentDbaseRecord2.FIETSHEEN.Value.ToBooleanFromDbaseValue() ?? false, segment2Geometry),
                 BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentDbaseRecord2.FIETSTERUG.Value.ToBooleanFromDbaseValue() ?? false, segment2Geometry),
                 PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(roadSegmentDbaseRecord2.VOETGANGER.Value.ToBooleanFromDbaseValue() ?? false, segment2Geometry),
                 EuropeanRoadNumbers = new List<EuropeanRoadNumber> { EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value!) },
                 NationalRoadNumbers = new List<NationalRoadNumber> { NationalRoadNumber.Parse(nationalRoadDbaseRecord.NWNUMMER.Value!) }
             })
             .AppendChange(new AddGradeSeparatedJunctionChange
             {
                 TemporaryId = new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                 Type = GradeSeparatedJunctionTypeV2.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value],
                 UpperRoadSegmentId = new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                 LowerRoadSegmentId = roadSegment2TemporaryId
             });

         using (zipArchive)
         {
             var sut = ZipArchiveFeatureCompareTranslatorV3Builder.Create();

             TranslatedChanges result = null;
             try
             {
                 result = await sut.TranslateAsync(zipArchive, ZipArchiveMetadata.Empty.WithInwinning(), CancellationToken.None);

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

     private static Fixture CreateFixture(DomainV2ZipArchiveTestData testData)
     {
         var fixture = testData.Fixture;

         fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(composer => composer
             .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
             {
                 EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                 WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                 EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
             })
             .OmitAutoProperties());

         fixture.Customize<GradeSeparatedJunctionDbaseRecord>(composer => composer
             .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
             {
                 OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                 TYPE = { Value = (short)fixture.Create<GradeSeparatedJunctionTypeV2>().Translation.Identifier },
                 BO_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                 ON_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
             })
             .OmitAutoProperties());

         fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(composer => composer
             .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
             {
                 NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                 WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                 NWNUMMER = { Value = fixture.Create<NationalRoadNumber>().ToString() }
             })
             .OmitAutoProperties());

         fixture.Customize<RoadNodeDbaseRecord>(composer => composer
             .FromFactory(random => new RoadNodeDbaseRecord
             {
                 WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                 TYPE = { Value = (short)fixture.Create<RoadNodeTypeV2>().Translation.Identifier }
             })
             .OmitAutoProperties());

         fixture.Customize<RoadSegmentDbaseRecord>(composer => composer
             .FromFactory(random => new RoadSegmentDbaseRecord
             {
                 WS_TEMPID = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                 WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                 LBEHEER = { Value = fixture.Create<OrganizationId>() },
                 RBEHEER = { Value = fixture.Create<OrganizationId>() },
                 MORF = { Value = (short)fixture.Create<RoadSegmentMorphologyV2>().Translation.Identifier },
                 STATUS = { Value = fixture.Create<RoadSegmentStatusV2>().Translation.Identifier },
                 WEGCAT = { Value = fixture.Create<RoadSegmentCategoryV2>().Translation.Identifier },
                 LSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                 RSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                 TOEGANG = { Value = (short)fixture.Create<RoadSegmentAccessRestrictionV2>().Translation.Identifier },
                 VERHARDING = { Value = fixture.Create<RoadSegmentSurfaceTypeV2>().Translation.Identifier },
                 AUTOHEEN = { Value = new RoadSegmentId(random.Next(0, 2)) },
                 AUTOTERUG = { Value = new RoadSegmentId(random.Next(0, 2)) },
                 FIETSHEEN = { Value = new RoadSegmentId(random.Next(0, 2)) },
                 FIETSTERUG = { Value = new RoadSegmentId(random.Next(0, 2)) },
                 VOETGANGER = { Value = new RoadSegmentId(random.Next(0, 2)) },
             })
             .OmitAutoProperties());

         return fixture;
     }
 }
