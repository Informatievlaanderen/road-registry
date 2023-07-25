namespace RoadRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Projections;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    
    public class ExtractsZipArchiveBuilder
    {
        public Fixture Fixture { get; }

        private ExtractsZipArchiveIntegrationDataSetBuilder _integration;
        private ExtractsZipArchiveExtractDataSetBuilder _extract;
        private ExtractsZipArchiveChangeDataSetBuilder _change;

        private ZipArchiveIntegrationDataSetStreams _integrationStreams;
        private ZipArchiveDataSetStreams _extractStreams;
        private ZipArchiveDataSetStreams _changeStreams;

        private readonly ExtractsZipArchiveTestData _testData;

        public ExtractsZipArchiveBuilder(Action<Fixture> customize = null)
        {
            _testData = new ExtractsZipArchiveTestData();
            Fixture = CreateFixture(_testData);
            customize?.Invoke(Fixture);
        }

        public ExtractsZipArchiveBuilder WithIntegration(Action<ExtractsZipArchiveIntegrationDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            _integration ??= new ExtractsZipArchiveIntegrationDataSetBuilder(Fixture);
            _integration.ConfigureIntegration(configure);
            _integrationStreams = _integration.Build();

            return this;
        }

        public ExtractsZipArchiveBuilder WithExtract(Action<ExtractsZipArchiveExtractDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            if (_integrationStreams is null)
            {
                WithIntegration((_, _) => { });
            }

            _extract ??= new ExtractsZipArchiveExtractDataSetBuilder(Fixture);
            _extract.ConfigureExtract(configure);
            _extractStreams = _extract.Build();

            return this;
        }

        public ExtractsZipArchiveBuilder WithChange(Action<ExtractsZipArchiveExtractDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
        {
            if (_extractStreams is null)
            {
                WithExtract((_, _) => { });
            }
            
            _change ??= new ExtractsZipArchiveChangeDataSetBuilder(Fixture, _extract);
            _change.ConfigureChange(configure);
            _changeStreams = _change.Build();

            return this;
        }

        public class ZipArchiveBuildContext
        {
            public ZipArchive ZipArchive { get; init; }
            public ZipArchiveIntegrationBuildContextSet Integration { get; init; }
            public ZipArchiveBuildContextSet Extract { get; init; }
            public ZipArchiveBuildContextSet Change { get; init; }

            public RoadSegmentId GetMaxRoadSegmentId()
            {
                return new RoadSegmentId(Integration.DataSet.RoadSegmentDbaseRecords.Select(x => x.WS_OIDN.Value)
                    .Concat(Extract.DataSet.RoadSegmentDbaseRecords.Select(x => x.WS_OIDN.Value))
                    .Concat(Change.DataSet.RoadSegmentDbaseRecords.Select(x => x.WS_OIDN.Value))
                    .Max());
            }
        }

        public class ZipArchiveBuildContextSet
        {
            public ZipArchiveDataSet DataSet { get; init; }
            public ZipArchiveDataSetTestData TestData { get; init; }
        }
        public class ZipArchiveIntegrationBuildContextSet
        {
            public ZipArchiveIntegrationDataSet DataSet { get; init; }
        }

        public (ZipArchive, T) BuildWithResult<T>(Func<ZipArchiveBuildContext, T> build)
        {
            var zipArchive = Build();

            return (zipArchive, build(new ZipArchiveBuildContext
            {
                ZipArchive = zipArchive,
                Integration = new ZipArchiveIntegrationBuildContextSet
                {
                    DataSet = _integration.DataSet
                },
                Extract = new ZipArchiveBuildContextSet
                {
                    TestData = _extract.TestData,
                    DataSet = _extract.DataSet
                },
                Change = new ZipArchiveBuildContextSet
                {
                    TestData = _change.TestData,
                    DataSet = _change.DataSet
                }
            }));
        }

        public ZipArchive Build()
        {
            if (_changeStreams is null)
            {
                WithChange((_, _) => { });
            }
            
            return Fixture.CreateUploadZipArchive(_testData,
                roadSegmentProjectionFormatStream: Fixture.CreateProjectionFormatFileWithOneRecord(),
                roadNodeProjectionFormatStream: Fixture.CreateProjectionFormatFileWithOneRecord(),

                roadNodeDbaseIntegrationStream: _integrationStreams.RoadNodeDbaseRecords,
                roadNodeShapeIntegrationStream: _integrationStreams.RoadNodeShapeRecords,
                roadSegmentDbaseIntegrationStream: _integrationStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeIntegrationStream: _integrationStreams.RoadSegmentShapeRecords,
                
                roadNodeDbaseExtractStream: _extractStreams!.RoadNodeDbaseRecords,
                roadNodeShapeExtractStream: _extractStreams.RoadNodeShapeRecords,
                roadSegmentDbaseExtractStream: _extractStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeExtractStream: _extractStreams.RoadSegmentShapeRecords,
                europeanRoadExtractStream: _extractStreams.EuropeanRoadDbaseRecords,
                numberedRoadExtractStream: _extractStreams.NumberedRoadDbaseRecords,
                nationalRoadExtractStream: _extractStreams.NationalRoadDbaseRecords,
                laneExtractStream: _extractStreams.LaneDbaseRecords,
                surfaceExtractStream: _extractStreams.SurfaceDbaseRecords,
                widthExtractStream: _extractStreams.WidthDbaseRecords,
                gradeSeparatedJunctionExtractStream: _extractStreams.GradeSeparatedJunctionDbaseRecords,

                roadNodeDbaseChangeStream: _changeStreams!.RoadNodeDbaseRecords,
                roadNodeShapeChangeStream: _changeStreams.RoadNodeShapeRecords,
                roadSegmentDbaseChangeStream: _changeStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeChangeStream: _changeStreams.RoadSegmentShapeRecords,
                europeanRoadChangeStream: _changeStreams.EuropeanRoadDbaseRecords,
                numberedRoadChangeStream: _changeStreams.NumberedRoadDbaseRecords,
                nationalRoadChangeStream: _changeStreams.NationalRoadDbaseRecords,
                laneChangeStream: _changeStreams.LaneDbaseRecords,
                surfaceChangeStream: _changeStreams.SurfaceDbaseRecords,
                widthChangeStream: _changeStreams.WidthDbaseRecords,
                gradeSeparatedJunctionChangeStream: _changeStreams.GradeSeparatedJunctionDbaseRecords
            );
        }

        private Fixture CreateFixture(ExtractsZipArchiveTestData testData)
        {
            var fixture = testData.Fixture;

            fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
                    {
                        EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                    })
                    .OmitAutoProperties());

            fixture.Customize<GradeSeparatedJunctionDbaseRecord>(
                composer => composer
                    .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
                    {
                        OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                        TYPE =
                            { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                        BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
                    {
                        NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentNumberedRoadAttributeDbaseRecord
                    {
                        GW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        IDENT8 = { Value = fixture.Create<NumberedRoadNumber>().ToString() },
                        RICHTING =
                        {
                        Value = (short)fixture.Create<RoadSegmentNumberedRoadDirection>().Translation
                            .Identifier
                        },
                        VOLGNUMMER = { Value = fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32() }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadNodeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = fixture.Create<RoadNodeId>() },
                        TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>() },
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
                        LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                        RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                        TGBEP =
                        {
                            Value = (short)fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                        }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentLaneAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentLaneAttributeDbaseRecord
                    {
                        RS_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        AANTAL = { Value = (short)fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                        RICHTING =
                        {
                        Value = (short)fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                        }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentSurfaceAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentSurfaceAttributeDbaseRecord
                    {
                        WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            fixture.Customize<RoadSegmentWidthAttributeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentWidthAttributeDbaseRecord
                    {
                        WB_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                        VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                        BREEDTE = { Value = (short)fixture.Create<RoadSegmentWidth>().ToInt32() }
                    })
                    .OmitAutoProperties());

            return fixture;
        }
    }

    public class ExtractsZipArchiveIntegrationDataSetBuilder
    {
        public ZipArchiveIntegrationDataSet DataSet { get; private set; }

        private readonly Fixture _fixture;

        public ExtractsZipArchiveIntegrationDataSetBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }
        
        public ExtractsZipArchiveIntegrationDataSetBuilder ConfigureIntegration(Action<ExtractsZipArchiveIntegrationDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            DataSet ??= new ZipArchiveIntegrationDataSet
            {
                RoadNodeDbaseRecords = new[] { _fixture.Create<RoadNodeDbaseRecord>() }.ToList(),
                RoadNodeShapeRecords = new[] { _fixture.Create<PointShapeContent>() }.ToList(),
                RoadSegmentDbaseRecords = new[] { _fixture.Create<RoadSegmentDbaseRecord>() }.ToList(),
                RoadSegmentShapeRecords = new[] { _fixture.Create<PolyLineMShapeContent>() }.ToList()
            };
            
            configure(this, new ExtractsZipArchiveDataSetBuilderContext(_fixture));
            return this;
        }

        public ZipArchiveIntegrationDataSetStreams Build()
        {
            return new ZipArchiveIntegrationDataSetStreams(_fixture, DataSet);
        }
    }

    public class ExtractsZipArchiveExtractDataSetBuilder
    {
        public ZipArchiveDataSetTestData TestData { get; } = new();
        public ZipArchiveDataSet DataSet { get; private set; }

        private readonly Fixture _fixture;

        public ExtractsZipArchiveExtractDataSetBuilder(Fixture fixture)
        {
            _fixture = fixture;

            TestData.RoadNode1DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode1ShapeRecord = CreateRoadNodeShapeRecord();

            TestData.RoadNode2DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode2ShapeRecord = CreateRoadNodeShapeRecord();

            TestData.RoadNode3DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode3ShapeRecord = CreateRoadNodeShapeRecord();

            TestData.RoadNode4DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode4ShapeRecord = CreateRoadNodeShapeRecord();

            TestData.RoadSegment1DbaseRecord = CreateRoadSegmentDbaseRecord();
            TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = TestData.RoadNode1DbaseRecord.WK_OIDN.Value;
            TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = TestData.RoadNode2DbaseRecord.WK_OIDN.Value;
            var roadSegment1LineString = CreateRoadSegmentGeometry();
            TestData.RoadSegment1ShapeRecord = roadSegment1LineString.ToShapeContent();
            
            TestData.RoadSegment2DbaseRecord = CreateRoadSegmentDbaseRecord();
            TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = TestData.RoadNode3DbaseRecord.WK_OIDN.Value;
            TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = TestData.RoadNode4DbaseRecord.WK_OIDN.Value;
            var roadSegment2LineString = new LineString(new Coordinate[]
            {
                new CoordinateM(roadSegment1LineString.Coordinates[0].X + 1000, roadSegment1LineString.Coordinates[0].Y + 1000, roadSegment1LineString.Coordinates[0].M),
                new CoordinateM(roadSegment1LineString.Coordinates[1].X + 1000, roadSegment1LineString.Coordinates[1].Y + 1000, roadSegment1LineString.Coordinates[1].M)
            }) { SRID = roadSegment1LineString.SRID };
            TestData.RoadSegment2ShapeRecord = roadSegment2LineString.ToShapeContent();
        
            TestData.RoadSegment1EuropeanRoadDbaseRecord = CreateRoadSegmentEuropeanRoadDbaseRecord();
            TestData.RoadSegment1EuropeanRoadDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
         
            TestData.RoadSegment1NationalRoadDbaseRecord = CreateRoadSegmentNationalRoadDbaseRecord();
            TestData.RoadSegment1NationalRoadDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
          
            TestData.RoadSegment1NumberedRoadDbaseRecord = CreateRoadSegmentNumberedRoadDbaseRecord();
            TestData.RoadSegment1NumberedRoadDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
          
            TestData.RoadSegment1LaneDbaseRecord = CreateRoadSegmentLaneDbaseRecord();
            TestData.RoadSegment1LaneDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
         
            TestData.RoadSegment2LaneDbaseRecord = CreateRoadSegmentLaneDbaseRecord();
            TestData.RoadSegment2LaneDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2LaneDbaseRecord.VANPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Max;
           
            TestData.RoadSegment1SurfaceDbaseRecord = CreateRoadSegmentSurfaceDbaseRecord();
            TestData.RoadSegment1SurfaceDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
        
            TestData.RoadSegment2SurfaceDbaseRecord = CreateRoadSegmentSurfaceDbaseRecord();
            TestData.RoadSegment2SurfaceDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2SurfaceDbaseRecord.VANPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Max;
          
            TestData.RoadSegment1WidthDbaseRecord = CreateRoadSegmentWidthDbaseRecord();
            TestData.RoadSegment1WidthDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
          
            TestData.RoadSegment2WidthDbaseRecord = CreateRoadSegmentWidthDbaseRecord();
            TestData.RoadSegment2WidthDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2WidthDbaseRecord.VANPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Min;
            TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Shape.MeasureRange.Max;

            TestData.GradeSeparatedJunctionDbaseRecord = CreateGradeSeparatedJunctionDbaseRecord();
            TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
        }

        public RoadNodeDbaseRecord CreateRoadNodeDbaseRecord()
        {
            return _fixture.Create<RoadNodeDbaseRecord>();
        }
        public PointShapeContent CreateRoadNodeShapeRecord()
        {
            return _fixture.Create<PointShapeContent>();
        }
        public RoadSegmentDbaseRecord CreateRoadSegmentDbaseRecord()
        {
            return _fixture.Create<RoadSegmentDbaseRecord>();
        }
        public LineString CreateRoadSegmentGeometry()
        {
            return _fixture.Create<LineString>();
        }
        public RoadSegmentEuropeanRoadAttributeDbaseRecord CreateRoadSegmentEuropeanRoadDbaseRecord()
        {
            return _fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        }
        public RoadSegmentNationalRoadAttributeDbaseRecord CreateRoadSegmentNationalRoadDbaseRecord()
        {
            return _fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        }
        public RoadSegmentNumberedRoadAttributeDbaseRecord CreateRoadSegmentNumberedRoadDbaseRecord()
        {
            return _fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        }
        public RoadSegmentLaneAttributeDbaseRecord CreateRoadSegmentLaneDbaseRecord()
        {
            return _fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        }
        public RoadSegmentSurfaceAttributeDbaseRecord CreateRoadSegmentSurfaceDbaseRecord()
        {
            return _fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        }
        public RoadSegmentWidthAttributeDbaseRecord CreateRoadSegmentWidthDbaseRecord()
        {
            return _fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        }
        public GradeSeparatedJunctionDbaseRecord CreateGradeSeparatedJunctionDbaseRecord()
        {
            return _fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        }

        public ExtractsZipArchiveExtractDataSetBuilder ConfigureExtract(Action<ExtractsZipArchiveExtractDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            DataSet ??= new ZipArchiveDataSet
            {
                RoadNodeDbaseRecords = new[] { TestData.RoadNode1DbaseRecord, TestData.RoadNode2DbaseRecord, TestData.RoadNode3DbaseRecord, TestData.RoadNode4DbaseRecord }.ToList(),
                RoadNodeShapeRecords = new[] { TestData.RoadNode1ShapeRecord, TestData.RoadNode2ShapeRecord, TestData.RoadNode3ShapeRecord, TestData.RoadNode4ShapeRecord }.ToList(),
                RoadSegmentDbaseRecords = new[] { TestData.RoadSegment1DbaseRecord, TestData.RoadSegment2DbaseRecord }.ToList(),
                RoadSegmentShapeRecords = new[] { TestData.RoadSegment1ShapeRecord, TestData.RoadSegment2ShapeRecord }.ToList(),
                EuropeanRoadDbaseRecords = new[] { TestData.RoadSegment1EuropeanRoadDbaseRecord }.ToList(),
                NationalRoadDbaseRecords = new[] { TestData.RoadSegment1NationalRoadDbaseRecord }.ToList(),
                NumberedRoadDbaseRecords = new[] { TestData.RoadSegment1NumberedRoadDbaseRecord }.ToList(),
                LaneDbaseRecords = new[] { TestData.RoadSegment1LaneDbaseRecord, TestData.RoadSegment2LaneDbaseRecord }.ToList(),
                SurfaceDbaseRecords = new[] { TestData.RoadSegment1SurfaceDbaseRecord, TestData.RoadSegment2SurfaceDbaseRecord }.ToList(),
                WidthDbaseRecords = new[] { TestData.RoadSegment1WidthDbaseRecord, TestData.RoadSegment2WidthDbaseRecord }.ToList(),
                GradeSeparatedJunctionDbaseRecords = new[] { TestData.GradeSeparatedJunctionDbaseRecord }.ToList()
            };

            configure(this, new ExtractsZipArchiveDataSetBuilderContext(_fixture));
            return this;
        }

        public ZipArchiveDataSetStreams Build()
        {
            return new ZipArchiveDataSetStreams(_fixture, DataSet);
        }
    }

    public class ExtractsZipArchiveChangeDataSetBuilder : ExtractsZipArchiveExtractDataSetBuilder
    {
        private readonly ExtractsZipArchiveExtractDataSetBuilder _extractBuilder;

        public ExtractsZipArchiveChangeDataSetBuilder(Fixture fixture, ExtractsZipArchiveExtractDataSetBuilder extractBuilder)
            : base(fixture)
        {
            _extractBuilder = extractBuilder;

            var manager = new RecyclableMemoryStreamManager();
            var encoding = Encoding.UTF8;

            var testData = extractBuilder.TestData;
            TestData.RoadNode1DbaseRecord = testData.RoadNode1DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode1ShapeRecord = testData.RoadNode1ShapeRecord.Clone(manager, encoding);

            TestData.RoadNode2DbaseRecord = testData.RoadNode2DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode2ShapeRecord = testData.RoadNode2ShapeRecord.Clone(manager, encoding);

            TestData.RoadNode3DbaseRecord = testData.RoadNode3DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode3ShapeRecord = testData.RoadNode3ShapeRecord.Clone(manager, encoding);

            TestData.RoadNode4DbaseRecord = testData.RoadNode4DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode4ShapeRecord = testData.RoadNode4ShapeRecord.Clone(manager, encoding);

            TestData.RoadSegment1DbaseRecord = testData.RoadSegment1DbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment1ShapeRecord = testData.RoadSegment1ShapeRecord.Clone(manager, encoding);

            TestData.RoadSegment2DbaseRecord = testData.RoadSegment2DbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2ShapeRecord = testData.RoadSegment2ShapeRecord.Clone(manager, encoding);

            TestData.RoadSegment1EuropeanRoadDbaseRecord = testData.RoadSegment1EuropeanRoadDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment1NationalRoadDbaseRecord = testData.RoadSegment1NationalRoadDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment1NumberedRoadDbaseRecord = testData.RoadSegment1NumberedRoadDbaseRecord.Clone(manager, encoding);

            TestData.RoadSegment1LaneDbaseRecord = testData.RoadSegment1LaneDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2LaneDbaseRecord = testData.RoadSegment2LaneDbaseRecord.Clone(manager, encoding);

            TestData.RoadSegment1SurfaceDbaseRecord = testData.RoadSegment1SurfaceDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2SurfaceDbaseRecord = testData.RoadSegment2SurfaceDbaseRecord.Clone(manager, encoding);

            TestData.RoadSegment1WidthDbaseRecord = testData.RoadSegment1WidthDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2WidthDbaseRecord = testData.RoadSegment2WidthDbaseRecord.Clone(manager, encoding);

            TestData.GradeSeparatedJunctionDbaseRecord = testData.GradeSeparatedJunctionDbaseRecord.Clone(manager, encoding);
        }

        public ExtractsZipArchiveExtractDataSetBuilder ConfigureChange(Action<ExtractsZipArchiveChangeDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
        {
            ConfigureExtract((_, context) =>
            {
                configure(this, new ExtractsZipArchiveChangeDataSetBuilderContext(context, _extractBuilder));
            });

            return this;
        }
    }

    public class ExtractsZipArchiveDataSetBuilderContext
    {
        public Fixture Fixture { get; }

        public ExtractsZipArchiveDataSetBuilderContext(Fixture fixture)
        {
            Fixture = fixture;
        }
    }
    public class ExtractsZipArchiveChangeDataSetBuilderContext : ExtractsZipArchiveDataSetBuilderContext
    {
        public ExtractsZipArchiveExtractDataSetBuilder Extract { get; }

        public ExtractsZipArchiveChangeDataSetBuilderContext(ExtractsZipArchiveDataSetBuilderContext context, ExtractsZipArchiveExtractDataSetBuilder extractsBuilder)
            : base(context.Fixture)
        {
            Extract = extractsBuilder;
        }
    }

    public class ZipArchiveDataSetTestData
    {
        public RoadNodeDbaseRecord RoadNode1DbaseRecord { get; set; }
        public PointShapeContent RoadNode1ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode2DbaseRecord { get; set; }
        public PointShapeContent RoadNode2ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode3DbaseRecord { get; set; }
        public PointShapeContent RoadNode3ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode4DbaseRecord { get; set; }
        public PointShapeContent RoadNode4ShapeRecord { get; set; }

        public RoadSegmentDbaseRecord RoadSegment1DbaseRecord { get; set; }
        public PolyLineMShapeContent RoadSegment1ShapeRecord { get; set; }
        public RoadSegmentDbaseRecord RoadSegment2DbaseRecord { get; set; }
        public PolyLineMShapeContent RoadSegment2ShapeRecord { get; set; }

        public RoadSegmentEuropeanRoadAttributeDbaseRecord RoadSegment1EuropeanRoadDbaseRecord { get; set; }
        public RoadSegmentNationalRoadAttributeDbaseRecord RoadSegment1NationalRoadDbaseRecord { get; set; }
        public RoadSegmentNumberedRoadAttributeDbaseRecord RoadSegment1NumberedRoadDbaseRecord { get; set; }
        public RoadSegmentLaneAttributeDbaseRecord RoadSegment1LaneDbaseRecord { get; set; }
        public RoadSegmentLaneAttributeDbaseRecord RoadSegment2LaneDbaseRecord { get; set; }
        public RoadSegmentSurfaceAttributeDbaseRecord RoadSegment1SurfaceDbaseRecord { get; set; }
        public RoadSegmentSurfaceAttributeDbaseRecord RoadSegment2SurfaceDbaseRecord { get; set; }
        public RoadSegmentWidthAttributeDbaseRecord RoadSegment1WidthDbaseRecord { get; set; }
        public RoadSegmentWidthAttributeDbaseRecord RoadSegment2WidthDbaseRecord { get; set; }

        public GradeSeparatedJunctionDbaseRecord GradeSeparatedJunctionDbaseRecord { get; set; }
    }

    public class ZipArchiveIntegrationDataSet
    {
        public List<RoadNodeDbaseRecord> RoadNodeDbaseRecords { get; set; }
        public List<PointShapeContent> RoadNodeShapeRecords { get; set; }
        public List<RoadSegmentDbaseRecord> RoadSegmentDbaseRecords { get; set; }
        public List<PolyLineMShapeContent> RoadSegmentShapeRecords { get; set; }
    }

    public class ZipArchiveDataSet: ZipArchiveIntegrationDataSet
    {
        public List<RoadSegmentEuropeanRoadAttributeDbaseRecord> EuropeanRoadDbaseRecords { get; set; }
        public List<RoadSegmentNationalRoadAttributeDbaseRecord> NationalRoadDbaseRecords { get; set; }
        public List<RoadSegmentNumberedRoadAttributeDbaseRecord> NumberedRoadDbaseRecords { get; set; }
        public List<RoadSegmentLaneAttributeDbaseRecord> LaneDbaseRecords { get; set; }
        public List<RoadSegmentSurfaceAttributeDbaseRecord> SurfaceDbaseRecords { get; set; }
        public List<RoadSegmentWidthAttributeDbaseRecord> WidthDbaseRecords { get; set; }
        public List<GradeSeparatedJunctionDbaseRecord> GradeSeparatedJunctionDbaseRecords { get; set; }
    }

    public class ZipArchiveIntegrationDataSetStreams
    {
        public ZipArchiveIntegrationDataSetStreams(Fixture fixture, ZipArchiveIntegrationDataSet set)
        {
            RoadNodeDbaseRecords = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, set.RoadNodeDbaseRecords ?? new List<RoadNodeDbaseRecord>());
            RoadNodeShapeRecords = fixture.CreateRoadNodeShapeFile(set.RoadNodeShapeRecords ?? new List<PointShapeContent>());
            RoadSegmentDbaseRecords = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, set.RoadSegmentDbaseRecords ?? new List<RoadSegmentDbaseRecord>());
            RoadSegmentShapeRecords = fixture.CreateRoadSegmentShapeFile(set.RoadSegmentShapeRecords ?? new List<PolyLineMShapeContent>());
        }

        public MemoryStream RoadNodeDbaseRecords { get; }
        public MemoryStream RoadNodeShapeRecords { get; }
        public MemoryStream RoadSegmentDbaseRecords { get; }
        public MemoryStream RoadSegmentShapeRecords { get; }
    }

    public class ZipArchiveDataSetStreams: ZipArchiveIntegrationDataSetStreams
    {
        public ZipArchiveDataSetStreams(Fixture fixture, ZipArchiveDataSet set)
            : base(fixture, set)
        {
            EuropeanRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, set.EuropeanRoadDbaseRecords ?? new List<RoadSegmentEuropeanRoadAttributeDbaseRecord>());
            NationalRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, set.NationalRoadDbaseRecords ?? new List<RoadSegmentNationalRoadAttributeDbaseRecord>());
            NumberedRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, set.NumberedRoadDbaseRecords ?? new List<RoadSegmentNumberedRoadAttributeDbaseRecord>());
            LaneDbaseRecords = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, set.LaneDbaseRecords ?? new List<RoadSegmentLaneAttributeDbaseRecord>());
            SurfaceDbaseRecords = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, set.SurfaceDbaseRecords ?? new List<RoadSegmentSurfaceAttributeDbaseRecord>());
            WidthDbaseRecords = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, set.WidthDbaseRecords ?? new List<RoadSegmentWidthAttributeDbaseRecord>());
            GradeSeparatedJunctionDbaseRecords = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, set.GradeSeparatedJunctionDbaseRecords ?? new List<GradeSeparatedJunctionDbaseRecord>());
        }

        public MemoryStream EuropeanRoadDbaseRecords { get; }
        public MemoryStream NationalRoadDbaseRecords { get; }
        public MemoryStream NumberedRoadDbaseRecords { get; }
        public MemoryStream LaneDbaseRecords { get; }
        public MemoryStream SurfaceDbaseRecords { get; }
        public MemoryStream WidthDbaseRecords { get; }
        public MemoryStream GradeSeparatedJunctionDbaseRecords { get; }
    }
}
