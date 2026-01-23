namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;

using GradeSeparatedJunction.Changes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Tests.BackOffice.Extracts.V2;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public class AllScenarios : FeatureCompareTranslatorScenariosBase
{
    public AllScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task Added()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadNode3DbaseRecord, builder.TestData.RoadNode4DbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadNode3ShapeRecord, builder.TestData.RoadNode4ShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context =>
            {
                var maxRoadSegmentId = context.GetMaxRoadSegmentId();
                var roadSegment1TemporaryId = maxRoadSegmentId.Next();

                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddRoadNodeChange
                        {
                            TemporaryId = new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                            OriginalId = null,
                            Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value],
                            Geometry = context.Change.TestData.RoadNode1ShapeRecord.Geometry.ToRoadNodeGeometry()
                        }
                    )
                    .AppendChange(
                        new AddRoadNodeChange {
                            TemporaryId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                            OriginalId = null,
                            Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode2DbaseRecord.TYPE.Value],
                            Geometry = context.Change.TestData.RoadNode2ShapeRecord.Geometry.ToRoadNodeGeometry()
                        }
                    )
                    .AppendChange(
                        new AddRoadSegmentChange {
                            TemporaryId = roadSegment1TemporaryId,
                            OriginalId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            StartNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                            EndNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry.ToRoadSegmentGeometry(),
                            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value!)),
                            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value]),
                            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value]),
                            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value]),
                            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value]),
                            StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value), StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)),
                            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>()
                                .Add(new(new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))),
                                    RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value]),
                            EuropeanRoadNumbers = [
                                EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!),
                                EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)
                            ],
                            NationalRoadNumbers = [
                                NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!),
                                NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value!)
                            ]
                        }
                    )
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionType.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            UpperRoadSegmentId = roadSegment1TemporaryId,
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Modified()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadNode1DbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadNodeType.ByIdentifier[builder.TestData.RoadNode1DbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.TestData.RoadNode1ShapeRecord.Geometry = new Point(builder.TestData.RoadNode1ShapeRecord.Geometry.X + 0.01, builder.TestData.RoadNode1ShapeRecord.Geometry.Y)
                    .FillSridIfMissing();

                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = builder.TestData.RoadNode3DbaseRecord.WK_OIDN.Value;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = builder.TestData.RoadNode4DbaseRecord.WK_OIDN.Value;
                builder.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value = fixture.CreateWhichIsDifferentThan(new OrganizationId(builder.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value!));
                builder.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentMorphology.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.STATUS.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentCategory.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.TGBEP.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentAccessRestriction.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.TGBEP.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = fixture.CreateWhichIsDifferentThan(new StreetNameLocalId(builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value!.Value));
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = fixture.CreateWhichIsDifferentThan(new StreetNameLocalId(builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value!.Value));
                var lineString = new LineString([
                    new CoordinateM(builder.TestData.RoadNode1ShapeRecord.Geometry.X, builder.TestData.RoadNode1ShapeRecord.Geometry.Y, builder.TestData.RoadNode1ShapeRecord.Geometry.X),
                    new CoordinateM(builder.TestData.RoadNode2ShapeRecord.Geometry.X, builder.TestData.RoadNode2ShapeRecord.Geometry.Y, builder.TestData.RoadNode2ShapeRecord.Geometry.X)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;

                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(EuropeanRoadNumber.Parse(builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!), EuropeanRoadNumber.Parse(builder.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)).ToString();
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value = fixture.CreateWhichIsDifferentThan(NationalRoadNumber.Parse(builder.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!), NationalRoadNumber.Parse(builder.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value!)).ToString();
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentSurfaceType.ByIdentifier[builder.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                            Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value],
                            Geometry = context.Change.TestData.RoadNode1ShapeRecord.Geometry.ToRoadNodeGeometry()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadSegmentChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            StartNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                            EndNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry.ToRoadSegmentGeometry(),
                            //GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value!)),
                            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value]),
                            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value]),
                            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value]),
                            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value]),
                            StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value), StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)),
                            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                                RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value]
                            )
                        }
                    )
                    .AppendChange(
                        new AddRoadSegmentToEuropeanRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            Number = EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromEuropeanRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            Number = EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new AddRoadSegmentToNationalRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            Number = NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!)
                        }
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromNationalRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            Number = NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!)
                        }
                    )
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionType.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            UpperRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value)
                        }
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task NoChanges()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Removed()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadNode3DbaseRecord, builder.TestData.RoadNode4DbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadNode3ShapeRecord, builder.TestData.RoadNode4ShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadNode1DbaseRecord.WK_OIDN.Value)
                    }
                )
                .AppendChange(
                    new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadNode2DbaseRecord.WK_OIDN.Value)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentFromEuropeanRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentFromEuropeanRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentFromNationalRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentFromNationalRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value!)
                    }
                )
                .AppendChange(
                    new RemoveGradeSeparatedJunctionChange
                    {
                        GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                    }
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
