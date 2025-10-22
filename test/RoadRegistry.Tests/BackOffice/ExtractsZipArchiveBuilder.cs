namespace RoadRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using Editor.Schema.Extensions;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Extracts.Dbase;
    using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadSegment.ValueObjects;
    using Point = NetTopologySuite.Geometries.Point;

    public class ExtractsZipArchiveBuilder
    {
        public Fixture Fixture { get; }
        public RecordBuilder Records { get; }

        private ExtractsZipArchiveIntegrationDataSetBuilder _integration;
        private ExtractsZipArchiveExtractDataSetBuilder _extract;
        private ExtractsZipArchiveChangeDataSetBuilder _change;

        private ZipArchiveIntegrationDataSetStreams _integrationStreams;
        private ZipArchiveDataSetStreams _extractStreams;
        private ZipArchiveDataSetStreams _changeStreams;

        private readonly List<string> _excludeFileNames = new();

        private readonly ExtractsZipArchiveTestData _testData;

        public ExtractsZipArchiveBuilder(Action<Fixture> customize = null)
        {
            _testData = new ExtractsZipArchiveTestData();
            Fixture = CreateFixture(_testData);
            customize?.Invoke(Fixture);
            Records = new RecordBuilder(Fixture);
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

        public ExtractsZipArchiveBuilder WithChange(Action<ExtractsZipArchiveChangeDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
        {
            if (_extractStreams is null)
            {
                WithExtract((_, _) => { });
            }

            _change ??= new ExtractsZipArchiveChangeDataSetBuilder(Fixture, _extract, _integration);
            _change.ConfigureChange(configure);
            _changeStreams = _change.Build();

            return this;
        }

        public ExtractsZipArchiveBuilder ExcludeFileNames(params string[] fileNames)
        {
            _excludeFileNames.AddRange(fileNames);
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

        public (ZipArchive, T) BuildWithResult<T>(Func<ZipArchiveBuildContext, T> build, MemoryStream archiveStream = null)
        {
            var zipArchive = Build(archiveStream);

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

        public MemoryStream BuildArchiveStream()
        {
            var archiveStream = new MemoryStream();

            var archive = Build(archiveStream);
            archive.Dispose();

            archiveStream.Position = 0;
            return archiveStream;
        }

        public ZipArchive Build(MemoryStream archiveStream = null)
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
                gradeSeparatedJunctionChangeStream: _changeStreams.GradeSeparatedJunctionDbaseRecords,

                transactionZoneStream: _changeStreams.TransactionZoneDbaseRecords,
                archiveStream: archiveStream,
                excludeFileNames: _excludeFileNames
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
                        LSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                        RSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
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
                RoadNodeShapeRecords = new[] { _fixture.Create<RoadNodeShapeRecord>() }.ToList(),
                RoadSegmentDbaseRecords = new[] { _fixture.Create<RoadSegmentDbaseRecord>() }.ToList(),
                RoadSegmentShapeRecords = new[] { _fixture.Create<RoadSegmentShapeRecord>() }.ToList()
            };

            configure(this, new ExtractsZipArchiveDataSetBuilderContext(_fixture));
            return this;
        }

        public ZipArchiveIntegrationDataSetStreams Build()
        {
            return new ZipArchiveIntegrationDataSetStreams(_fixture, DataSet);
        }
    }

    public class ExtractsZipArchiveExtractDataSetBuilder: RecordBuilder
    {
        public ZipArchiveDataSetTestData TestData { get; } = new();
        public ZipArchiveDataSet DataSet { get; private set; }

        private readonly Fixture _fixture;

        public ExtractsZipArchiveExtractDataSetBuilder(Fixture fixture)
            : base(fixture)
        {
            _fixture = fixture;

            TestData.RoadNode1DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode2DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode3DbaseRecord = CreateRoadNodeDbaseRecord();
            TestData.RoadNode4DbaseRecord = CreateRoadNodeDbaseRecord();

            TestData.RoadSegment1DbaseRecord = CreateRoadSegmentDbaseRecord();
            TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = TestData.RoadNode1DbaseRecord.WK_OIDN.Value;
            TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = TestData.RoadNode2DbaseRecord.WK_OIDN.Value;
            var roadSegment1LineString = CreateRoadSegmentGeometry();
            TestData.RoadNode1ShapeRecord = CreateRoadNodeShapeRecord(roadSegment1LineString.StartPoint);
            TestData.RoadNode2ShapeRecord = CreateRoadNodeShapeRecord(roadSegment1LineString.EndPoint);
            TestData.RoadSegment1ShapeRecord = CreateRoadSegmentShapeRecord(roadSegment1LineString);

            TestData.RoadSegment2DbaseRecord = CreateRoadSegmentDbaseRecord();
            TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = TestData.RoadNode3DbaseRecord.WK_OIDN.Value;
            TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = TestData.RoadNode4DbaseRecord.WK_OIDN.Value;
            var roadSegment2LineString = new LineString(new Coordinate[]
            {
                new CoordinateM(roadSegment1LineString.Coordinates[0].X + 1000, roadSegment1LineString.Coordinates[0].Y + 1000, roadSegment1LineString.Coordinates[0].M),
                new CoordinateM(roadSegment1LineString.Coordinates[1].X + 1000, roadSegment1LineString.Coordinates[1].Y + 1000, roadSegment1LineString.Coordinates[1].M)
            }) { SRID = roadSegment1LineString.SRID };
            TestData.RoadNode3ShapeRecord = CreateRoadNodeShapeRecord(roadSegment2LineString.StartPoint);
            TestData.RoadNode4ShapeRecord = CreateRoadNodeShapeRecord(roadSegment2LineString.EndPoint);
            TestData.RoadSegment2ShapeRecord = CreateRoadSegmentShapeRecord(roadSegment2LineString);

            TestData.RoadSegment1EuropeanRoadDbaseRecord1 = CreateRoadSegmentEuropeanRoadDbaseRecord();
            TestData.RoadSegment1EuropeanRoadDbaseRecord1.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1EuropeanRoadDbaseRecord2 = CreateRoadSegmentEuropeanRoadDbaseRecord();
            TestData.RoadSegment1EuropeanRoadDbaseRecord2.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(EuropeanRoadNumber.Parse(TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value));

            TestData.RoadSegment1NationalRoadDbaseRecord1 = CreateRoadSegmentNationalRoadDbaseRecord();
            TestData.RoadSegment1NationalRoadDbaseRecord1.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1NationalRoadDbaseRecord2 = CreateRoadSegmentNationalRoadDbaseRecord();
            TestData.RoadSegment1NationalRoadDbaseRecord2.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value = fixture.CreateWhichIsDifferentThan(NationalRoadNumber.Parse(TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value));

            TestData.RoadSegment1NumberedRoadDbaseRecord1 = CreateRoadSegmentNumberedRoadDbaseRecord();
            TestData.RoadSegment1NumberedRoadDbaseRecord1.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1NumberedRoadDbaseRecord2 = CreateRoadSegmentNumberedRoadDbaseRecord();
            TestData.RoadSegment1NumberedRoadDbaseRecord2.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value = fixture.CreateWhichIsDifferentThan(NumberedRoadNumber.Parse(TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value));

            TestData.RoadSegment1LaneDbaseRecord = CreateRoadSegmentLaneDbaseRecord();
            TestData.RoadSegment1LaneDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Geometry.Length;

            TestData.RoadSegment2LaneDbaseRecord = CreateRoadSegmentLaneDbaseRecord();
            TestData.RoadSegment2LaneDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2LaneDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Geometry.Length;

            TestData.RoadSegment1SurfaceDbaseRecord = CreateRoadSegmentSurfaceDbaseRecord();
            TestData.RoadSegment1SurfaceDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Geometry.Length;

            TestData.RoadSegment2SurfaceDbaseRecord = CreateRoadSegmentSurfaceDbaseRecord();
            TestData.RoadSegment2SurfaceDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2SurfaceDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Geometry.Length;

            TestData.RoadSegment1WidthDbaseRecord = CreateRoadSegmentWidthDbaseRecord();
            TestData.RoadSegment1WidthDbaseRecord.WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = TestData.RoadSegment1ShapeRecord.Geometry.Length;

            TestData.RoadSegment2WidthDbaseRecord = CreateRoadSegmentWidthDbaseRecord();
            TestData.RoadSegment2WidthDbaseRecord.WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;
            TestData.RoadSegment2WidthDbaseRecord.VANPOS.Value = 0;
            TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = TestData.RoadSegment2ShapeRecord.Geometry.Length;

            TestData.GradeSeparatedJunctionDbaseRecord = CreateGradeSeparatedJunctionDbaseRecord();
            TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = TestData.RoadSegment2DbaseRecord.WS_OIDN.Value;

            TestData.TransactionZoneDbaseRecord = CreateTransactionZoneDbaseRecord();
        }

        public ExtractsZipArchiveExtractDataSetBuilder ConfigureExtract(Action<ExtractsZipArchiveExtractDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            DataSet ??= new ZipArchiveDataSet
            {
                RoadNodeDbaseRecords = new[] { TestData.RoadNode1DbaseRecord, TestData.RoadNode2DbaseRecord, TestData.RoadNode3DbaseRecord, TestData.RoadNode4DbaseRecord }.ToList(),
                RoadNodeShapeRecords = new[] { TestData.RoadNode1ShapeRecord, TestData.RoadNode2ShapeRecord, TestData.RoadNode3ShapeRecord, TestData.RoadNode4ShapeRecord }.ToList(),
                RoadSegmentDbaseRecords = new[] { TestData.RoadSegment1DbaseRecord, TestData.RoadSegment2DbaseRecord }.ToList(),
                RoadSegmentShapeRecords = new[] { TestData.RoadSegment1ShapeRecord, TestData.RoadSegment2ShapeRecord }.ToList(),
                EuropeanRoadDbaseRecords = new[] { TestData.RoadSegment1EuropeanRoadDbaseRecord1, TestData.RoadSegment1EuropeanRoadDbaseRecord2 }.ToList(),
                NationalRoadDbaseRecords = new[] { TestData.RoadSegment1NationalRoadDbaseRecord1, TestData.RoadSegment1NationalRoadDbaseRecord2 }.ToList(),
                NumberedRoadDbaseRecords = new[] { TestData.RoadSegment1NumberedRoadDbaseRecord1, TestData.RoadSegment1NumberedRoadDbaseRecord2 }.ToList(),
                LaneDbaseRecords = new[] { TestData.RoadSegment1LaneDbaseRecord, TestData.RoadSegment2LaneDbaseRecord }.ToList(),
                SurfaceDbaseRecords = new[] { TestData.RoadSegment1SurfaceDbaseRecord, TestData.RoadSegment2SurfaceDbaseRecord }.ToList(),
                WidthDbaseRecords = new[] { TestData.RoadSegment1WidthDbaseRecord, TestData.RoadSegment2WidthDbaseRecord }.ToList(),
                GradeSeparatedJunctionDbaseRecords = new[] { TestData.GradeSeparatedJunctionDbaseRecord }.ToList(),
                TransactionZoneDbaseRecords = new[] { TestData.TransactionZoneDbaseRecord }.ToList()
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
        private readonly ExtractsZipArchiveIntegrationDataSetBuilder _integrationBuilder;

        public ExtractsZipArchiveChangeDataSetBuilder(Fixture fixture, ExtractsZipArchiveExtractDataSetBuilder extractBuilder, ExtractsZipArchiveIntegrationDataSetBuilder integrationBuilder)
            : base(fixture)
        {
            _extractBuilder = extractBuilder;
            _integrationBuilder = integrationBuilder;

            var manager = new RecyclableMemoryStreamManager();
            var encoding = Encoding.UTF8;

            var testData = extractBuilder.TestData;
            TestData.RoadNode1DbaseRecord = testData.RoadNode1DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode1ShapeRecord = testData.RoadNode1ShapeRecord.Clone();

            TestData.RoadNode2DbaseRecord = testData.RoadNode2DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode2ShapeRecord = testData.RoadNode2ShapeRecord.Clone();

            TestData.RoadNode3DbaseRecord = testData.RoadNode3DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode3ShapeRecord = testData.RoadNode3ShapeRecord.Clone();

            TestData.RoadNode4DbaseRecord = testData.RoadNode4DbaseRecord.Clone(manager, encoding);
            TestData.RoadNode4ShapeRecord = testData.RoadNode4ShapeRecord.Clone();

            TestData.RoadSegment1DbaseRecord = testData.RoadSegment1DbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment1ShapeRecord = testData.RoadSegment1ShapeRecord.Clone();

            TestData.RoadSegment2DbaseRecord = testData.RoadSegment2DbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2ShapeRecord = testData.RoadSegment2ShapeRecord.Clone();

            TestData.RoadSegment1EuropeanRoadDbaseRecord1 = testData.RoadSegment1EuropeanRoadDbaseRecord1.Clone(manager, encoding);
            TestData.RoadSegment1EuropeanRoadDbaseRecord2 = testData.RoadSegment1EuropeanRoadDbaseRecord2.Clone(manager, encoding);
            TestData.RoadSegment1NationalRoadDbaseRecord1 = testData.RoadSegment1NationalRoadDbaseRecord1.Clone(manager, encoding);
            TestData.RoadSegment1NationalRoadDbaseRecord2 = testData.RoadSegment1NationalRoadDbaseRecord2.Clone(manager, encoding);
            TestData.RoadSegment1NumberedRoadDbaseRecord1 = testData.RoadSegment1NumberedRoadDbaseRecord1.Clone(manager, encoding);
            TestData.RoadSegment1NumberedRoadDbaseRecord2 = testData.RoadSegment1NumberedRoadDbaseRecord2.Clone(manager, encoding);

            TestData.RoadSegment1LaneDbaseRecord = testData.RoadSegment1LaneDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2LaneDbaseRecord = testData.RoadSegment2LaneDbaseRecord.Clone(manager, encoding);

            TestData.RoadSegment1SurfaceDbaseRecord = testData.RoadSegment1SurfaceDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2SurfaceDbaseRecord = testData.RoadSegment2SurfaceDbaseRecord.Clone(manager, encoding);

            TestData.RoadSegment1WidthDbaseRecord = testData.RoadSegment1WidthDbaseRecord.Clone(manager, encoding);
            TestData.RoadSegment2WidthDbaseRecord = testData.RoadSegment2WidthDbaseRecord.Clone(manager, encoding);

            TestData.GradeSeparatedJunctionDbaseRecord = testData.GradeSeparatedJunctionDbaseRecord.Clone(manager, encoding);
            TestData.TransactionZoneDbaseRecord = testData.TransactionZoneDbaseRecord.Clone(manager, encoding);
        }

        public ExtractsZipArchiveExtractDataSetBuilder ConfigureChange(Action<ExtractsZipArchiveChangeDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
        {
            ConfigureExtract((_, context) =>
            {
                configure(this, new ExtractsZipArchiveChangeDataSetBuilderContext(context, _extractBuilder, _integrationBuilder));
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
        public ExtractsZipArchiveIntegrationDataSetBuilder Integration { get; }

        public ExtractsZipArchiveChangeDataSetBuilderContext(ExtractsZipArchiveDataSetBuilderContext context,
            ExtractsZipArchiveExtractDataSetBuilder extractsBuilder,
            ExtractsZipArchiveIntegrationDataSetBuilder integrationBuilder)
            : base(context.Fixture)
        {
            Extract = extractsBuilder;
            Integration = integrationBuilder;
        }
    }

    public class RecordBuilder
    {
        private readonly Fixture _fixture;

        public RecordBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public RoadNodeDbaseRecord CreateRoadNodeDbaseRecord()
        {
            return _fixture.Create<RoadNodeDbaseRecord>();
        }
        public RoadNodeShapeRecord CreateRoadNodeShapeRecord()
        {
            return CreateRoadNodeShapeRecord(_fixture.Create<Point>());
        }
        public RoadNodeShapeRecord CreateRoadNodeShapeRecord(Point point)
        {
            return new RoadNodeShapeRecord
            {
                Geometry = point
            };
        }
        public RoadSegmentDbaseRecord CreateRoadSegmentDbaseRecord()
        {
            return _fixture.Create<RoadSegmentDbaseRecord>();
        }
        public RoadSegmentShapeRecord CreateRoadSegmentShapeRecord()
        {
            return CreateRoadSegmentShapeRecord(CreateRoadSegmentGeometry());
        }
        public RoadSegmentShapeRecord CreateRoadSegmentShapeRecord(LineString lineString)
        {
            return new RoadSegmentShapeRecord
            {
                Geometry = lineString.ToMultiLineString()
            };
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
        public TransactionZoneDbaseRecord CreateTransactionZoneDbaseRecord()
        {
            return _fixture.Create<TransactionZoneDbaseRecord>();
        }
    }

    public class RoadSegmentShapeRecord
    {
        public MultiLineString Geometry { get; set; }

        public RoadSegmentShapeRecord Clone()
        {
            return new RoadSegmentShapeRecord
            {
                Geometry = Geometry
            };
        }
    }
    public class RoadNodeShapeRecord
    {
        public Point Geometry { get; set; }

        public RoadNodeShapeRecord Clone()
        {
            return new RoadNodeShapeRecord
            {
                Geometry = Geometry
            };
        }
    }

    public class ZipArchiveDataSetTestData
    {
        public RoadNodeDbaseRecord RoadNode1DbaseRecord { get; set; }
        public RoadNodeShapeRecord RoadNode1ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode2DbaseRecord { get; set; }
        public RoadNodeShapeRecord RoadNode2ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode3DbaseRecord { get; set; }
        public RoadNodeShapeRecord RoadNode3ShapeRecord { get; set; }
        public RoadNodeDbaseRecord RoadNode4DbaseRecord { get; set; }
        public RoadNodeShapeRecord RoadNode4ShapeRecord { get; set; }

        public RoadSegmentDbaseRecord RoadSegment1DbaseRecord { get; set; }
        public RoadSegmentShapeRecord RoadSegment1ShapeRecord { get; set; }
        public RoadSegmentDbaseRecord RoadSegment2DbaseRecord { get; set; }
        public RoadSegmentShapeRecord RoadSegment2ShapeRecord { get; set; }

        public RoadSegmentEuropeanRoadAttributeDbaseRecord RoadSegment1EuropeanRoadDbaseRecord1 { get; set; }
        public RoadSegmentEuropeanRoadAttributeDbaseRecord RoadSegment1EuropeanRoadDbaseRecord2 { get; set; }
        public RoadSegmentNationalRoadAttributeDbaseRecord RoadSegment1NationalRoadDbaseRecord1 { get; set; }
        public RoadSegmentNationalRoadAttributeDbaseRecord RoadSegment1NationalRoadDbaseRecord2 { get; set; }
        public RoadSegmentNumberedRoadAttributeDbaseRecord RoadSegment1NumberedRoadDbaseRecord1 { get; set; }
        public RoadSegmentNumberedRoadAttributeDbaseRecord RoadSegment1NumberedRoadDbaseRecord2 { get; set; }
        public RoadSegmentLaneAttributeDbaseRecord RoadSegment1LaneDbaseRecord { get; set; }
        public RoadSegmentLaneAttributeDbaseRecord RoadSegment2LaneDbaseRecord { get; set; }
        public RoadSegmentSurfaceAttributeDbaseRecord RoadSegment1SurfaceDbaseRecord { get; set; }
        public RoadSegmentSurfaceAttributeDbaseRecord RoadSegment2SurfaceDbaseRecord { get; set; }
        public RoadSegmentWidthAttributeDbaseRecord RoadSegment1WidthDbaseRecord { get; set; }
        public RoadSegmentWidthAttributeDbaseRecord RoadSegment2WidthDbaseRecord { get; set; }

        public GradeSeparatedJunctionDbaseRecord GradeSeparatedJunctionDbaseRecord { get; set; }
        public TransactionZoneDbaseRecord TransactionZoneDbaseRecord { get; set; }
    }

    public class ZipArchiveIntegrationDataSet
    {
        public List<RoadNodeDbaseRecord> RoadNodeDbaseRecords { get; set; }
        public List<RoadNodeShapeRecord> RoadNodeShapeRecords { get; set; }
        public List<RoadSegmentDbaseRecord> RoadSegmentDbaseRecords { get; set; }
        public List<RoadSegmentShapeRecord> RoadSegmentShapeRecords { get; set; }

        public virtual void Clear()
        {
            RoadNodeDbaseRecords.Clear();
            RoadNodeShapeRecords.Clear();
            RoadSegmentDbaseRecords.Clear();
            RoadSegmentShapeRecords.Clear();
        }
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
        public List<TransactionZoneDbaseRecord> TransactionZoneDbaseRecords { get; set; }

        public override void Clear()
        {
            base.Clear();

            EuropeanRoadDbaseRecords.Clear();
            NationalRoadDbaseRecords.Clear();
            NumberedRoadDbaseRecords.Clear();
            LaneDbaseRecords.Clear();
            SurfaceDbaseRecords.Clear();
            WidthDbaseRecords.Clear();
            GradeSeparatedJunctionDbaseRecords.Clear();
        }

        public void RemoveRoadSegment(int id)
        {
            var roadSegmentDbaseRecordIndex = RoadSegmentDbaseRecords.FindIndex(x => x.WS_OIDN.Value == id);
            RoadSegmentDbaseRecords.RemoveAt(roadSegmentDbaseRecordIndex);
            RoadSegmentShapeRecords.RemoveAt(roadSegmentDbaseRecordIndex);
            LaneDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            SurfaceDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            WidthDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            EuropeanRoadDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            NationalRoadDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            NumberedRoadDbaseRecords.RemoveAll(x => x.WS_OIDN.Value == id);
            GradeSeparatedJunctionDbaseRecords.RemoveAll(x => x.BO_WS_OIDN.Value == id || x.ON_WS_OIDN.Value == id);
        }
    }

    public class ZipArchiveIntegrationDataSetStreams
    {
        public ZipArchiveIntegrationDataSetStreams(Fixture fixture, ZipArchiveIntegrationDataSet set)
        {
            RoadNodeDbaseRecords = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, set.RoadNodeDbaseRecords ?? new List<RoadNodeDbaseRecord>());
            RoadNodeShapeRecords = fixture.CreateRoadNodeShapeFile((set.RoadNodeShapeRecords ?? new List<RoadNodeShapeRecord>()).Select(x => x.Geometry.ToShapeContent()).ToList());
            RoadSegmentDbaseRecords = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, set.RoadSegmentDbaseRecords ?? new List<RoadSegmentDbaseRecord>());
            RoadSegmentShapeRecords = fixture.CreateRoadSegmentShapeFile((set.RoadSegmentShapeRecords ?? new List<RoadSegmentShapeRecord>()).Select(x => x.Geometry.ToShapeContent()).ToList());
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
            EuropeanRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, set.EuropeanRoadDbaseRecords ?? []);
            NationalRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, set.NationalRoadDbaseRecords ?? []);
            NumberedRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, set.NumberedRoadDbaseRecords ?? []);
            LaneDbaseRecords = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, set.LaneDbaseRecords ?? []);
            SurfaceDbaseRecords = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, set.SurfaceDbaseRecords ?? []);
            WidthDbaseRecords = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, set.WidthDbaseRecords ?? []);
            GradeSeparatedJunctionDbaseRecords = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, set.GradeSeparatedJunctionDbaseRecords ?? []);
            TransactionZoneDbaseRecords = fixture.CreateDbfFile(TransactionZoneDbaseRecord.Schema, set.TransactionZoneDbaseRecords ?? []);
        }

        public MemoryStream EuropeanRoadDbaseRecords { get; }
        public MemoryStream NationalRoadDbaseRecords { get; }
        public MemoryStream NumberedRoadDbaseRecords { get; }
        public MemoryStream LaneDbaseRecords { get; }
        public MemoryStream SurfaceDbaseRecords { get; }
        public MemoryStream WidthDbaseRecords { get; }
        public MemoryStream GradeSeparatedJunctionDbaseRecords { get; }
        public MemoryStream TransactionZoneDbaseRecords { get; }
    }
}
