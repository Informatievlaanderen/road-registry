namespace RoadRegistry.Tests.BackOffice.Extracts.DomainV2
{
    using System.IO.Compression;
    using System.Text;
    using AutoFixture;
    using Editor.Schema.Extensions;
    using Extensions;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Extracts.Schemas.Inwinning;
    using RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;
    using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
    using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
    using Point = NetTopologySuite.Geometries.Point;

    public class DomainV2ZipArchiveBuilder
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
        private readonly DomainV2ZipArchiveTestData _testData;

        public DomainV2ZipArchiveBuilder(Action<Fixture> customize = null)
        {
            _testData = new DomainV2ZipArchiveTestData();
            Fixture = CreateFixture(_testData);
            customize?.Invoke(Fixture);
            Records = new RecordBuilder(Fixture);
        }

        public DomainV2ZipArchiveBuilder WithIntegration(Action<ExtractsZipArchiveIntegrationDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
        {
            _integration ??= new ExtractsZipArchiveIntegrationDataSetBuilder(Fixture);
            _integration.ConfigureIntegration(configure);
            _integrationStreams = _integration.Build();

            return this;
        }

        public DomainV2ZipArchiveBuilder WithExtract(Action<ExtractsZipArchiveExtractDataSetBuilder, ExtractsZipArchiveDataSetBuilderContext> configure)
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

        public DomainV2ZipArchiveBuilder WithChange(Action<ExtractsZipArchiveChangeDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
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

        public DomainV2ZipArchiveBuilder ExcludeFileNames(params string[] fileNames)
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

        public ZipArchive Build(MemoryStream archiveStream = null,
            MemoryStream roadSegmentProjectionFormatStream = null,
            MemoryStream roadNodeProjectionFormatStream = null,
            MemoryStream transactionZoneProjectionFormatStream = null)
        {
            if (_changeStreams is null)
            {
                WithChange((_, _) => { });
            }

            return Fixture.CreateUploadZipArchiveV2(
                _testData,
                roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream ?? Fixture.CreateLambert08ProjectionFormatFileWithOneRecord(),
                roadNodeProjectionFormatStream: roadNodeProjectionFormatStream ?? Fixture.CreateLambert08ProjectionFormatFileWithOneRecord(),
                transactionZoneProjectionFormatStream: transactionZoneProjectionFormatStream ?? Fixture.CreateLambert08ProjectionFormatFileWithOneRecord(),
                roadNodeDbaseIntegrationStream: _integrationStreams.RoadNodeDbaseRecords,
                roadNodeShapeIntegrationStream: _integrationStreams.RoadNodeShapeRecords,
                roadSegmentDbaseIntegrationStream: _integrationStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeIntegrationStream: _integrationStreams.RoadSegmentShapeRecords,
                roadNodeDbaseExtractStream: _extractStreams!.RoadNodeDbaseRecords,
                roadNodeShapeExtractStream: _extractStreams.RoadNodeShapeRecords,
                roadSegmentDbaseExtractStream: _extractStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeExtractStream: _extractStreams.RoadSegmentShapeRecords,
                europeanRoadExtractStream: _extractStreams.EuropeanRoadDbaseRecords,
                nationalRoadExtractStream: _extractStreams.NationalRoadDbaseRecords,
                gradeSeparatedJunctionExtractStream: _extractStreams.GradeSeparatedJunctionDbaseRecords,
                roadNodeDbaseChangeStream: _changeStreams!.RoadNodeDbaseRecords,
                roadNodeShapeChangeStream: _changeStreams.RoadNodeShapeRecords,
                roadSegmentDbaseChangeStream: _changeStreams.RoadSegmentDbaseRecords,
                roadSegmentShapeChangeStream: _changeStreams.RoadSegmentShapeRecords,
                europeanRoadChangeStream: _changeStreams.EuropeanRoadDbaseRecords,
                nationalRoadChangeStream: _changeStreams.NationalRoadDbaseRecords,
                gradeSeparatedJunctionChangeStream: _changeStreams.GradeSeparatedJunctionDbaseRecords,
                transactionZoneStream: _changeStreams.TransactionZoneDbaseRecords,
                transactionZoneShapeStream: _changeStreams.TransactionZoneShapeRecords,
                archiveStream: archiveStream,
                excludeFileNames: _excludeFileNames
            );
        }

        private Fixture CreateFixture(DomainV2ZipArchiveTestData testData)
        {
            var fixture = testData.Fixture;
            fixture.CustomizeUniqueInteger();

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

    public class ExtractsZipArchiveExtractDataSetBuilder : RecordBuilder
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
            var roadSegment1LineString = CreateRoadSegmentGeometry();
            TestData.RoadNode1ShapeRecord = CreateRoadNodeShapeRecord(roadSegment1LineString.StartPoint);
            TestData.RoadNode2ShapeRecord = CreateRoadNodeShapeRecord(roadSegment1LineString.EndPoint);
            TestData.RoadSegment1ShapeRecord = CreateRoadSegmentShapeRecord(roadSegment1LineString);

            TestData.RoadSegment2DbaseRecord = CreateRoadSegmentDbaseRecord();
            var roadSegment2LineString = new LineString([
                new CoordinateM(roadSegment1LineString.Coordinates[0].X + 1000, roadSegment1LineString.Coordinates[0].Y + 1000, roadSegment1LineString.Coordinates[0].M),
                new CoordinateM(roadSegment1LineString.Coordinates[1].X + 1000, roadSegment1LineString.Coordinates[1].Y + 1000, roadSegment1LineString.Coordinates[1].M)
            ]) { SRID = roadSegment1LineString.SRID };
            TestData.RoadNode3ShapeRecord = CreateRoadNodeShapeRecord(roadSegment2LineString.StartPoint);
            TestData.RoadNode4ShapeRecord = CreateRoadNodeShapeRecord(roadSegment2LineString.EndPoint);
            TestData.RoadSegment2ShapeRecord = CreateRoadSegmentShapeRecord(roadSegment2LineString);

            TestData.RoadSegment1EuropeanRoadDbaseRecord1 = CreateRoadSegmentEuropeanRoadDbaseRecord();
            TestData.RoadSegment1EuropeanRoadDbaseRecord1.WS_TEMPID.Value = TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            TestData.RoadSegment1EuropeanRoadDbaseRecord2 = CreateRoadSegmentEuropeanRoadDbaseRecord();
            TestData.RoadSegment1EuropeanRoadDbaseRecord2.WS_TEMPID.Value = TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(EuropeanRoadNumber.Parse(TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!));

            TestData.RoadSegment1NationalRoadDbaseRecord1 = CreateRoadSegmentNationalRoadDbaseRecord();
            TestData.RoadSegment1NationalRoadDbaseRecord1.WS_TEMPID.Value = TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            TestData.RoadSegment1NationalRoadDbaseRecord2 = CreateRoadSegmentNationalRoadDbaseRecord();
            TestData.RoadSegment1NationalRoadDbaseRecord2.WS_TEMPID.Value = TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            TestData.RoadSegment1NationalRoadDbaseRecord2.NWNUMMER.Value = fixture.CreateWhichIsDifferentThan(NationalRoadNumber.Parse(TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!));

            TestData.GradeSeparatedJunctionDbaseRecord = CreateGradeSeparatedJunctionDbaseRecord();
            TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value;

            TestData.TransactionZoneDbaseRecord = CreateTransactionZoneDbaseRecord();
            TestData.TransactionZoneShapeRecord = CreateTransactionZoneShapeRecord();
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
                GradeSeparatedJunctionDbaseRecords = new[] { TestData.GradeSeparatedJunctionDbaseRecord }.ToList(),
                TransactionZoneDbaseRecords = new[] { TestData.TransactionZoneDbaseRecord }.ToList(),
                TransactionZoneShapeRecords = new[] { TestData.TransactionZoneShapeRecord }.ToList(),
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

            TestData.GradeSeparatedJunctionDbaseRecord = testData.GradeSeparatedJunctionDbaseRecord.Clone(manager, encoding);
            TestData.TransactionZoneDbaseRecord = testData.TransactionZoneDbaseRecord.Clone(manager, encoding);
            TestData.TransactionZoneShapeRecord = testData.TransactionZoneShapeRecord.Clone();
        }

        public ExtractsZipArchiveExtractDataSetBuilder ConfigureChange(Action<ExtractsZipArchiveChangeDataSetBuilder, ExtractsZipArchiveChangeDataSetBuilderContext> configure)
        {
            ConfigureExtract((_, context) => { configure(this, new ExtractsZipArchiveChangeDataSetBuilderContext(context, _extractBuilder, _integrationBuilder)); });

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

        public GradeSeparatedJunctionDbaseRecord CreateGradeSeparatedJunctionDbaseRecord()
        {
            return _fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        }

        public TransactionZoneDbaseRecord CreateTransactionZoneDbaseRecord()
        {
            return _fixture.Create<TransactionZoneDbaseRecord>();
        }

        public TransactionZoneShapeRecord CreateTransactionZoneShapeRecord()
        {
            return CreateTransactionZoneShapeRecord(CreateTransactionZoneGeometry());
        }

        public TransactionZoneShapeRecord CreateTransactionZoneShapeRecord(Polygon polygon)
        {
            return new TransactionZoneShapeRecord
            {
                Geometry = polygon
            };
        }

        public Polygon CreateTransactionZoneGeometry()
        {
            return _fixture.Create<Polygon>();
        }
    }

    public class TransactionZoneShapeRecord
    {
        public Polygon Geometry { get; set; }

        public TransactionZoneShapeRecord Clone()
        {
            return new TransactionZoneShapeRecord
            {
                Geometry = Geometry
            };
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
        public GradeSeparatedJunctionDbaseRecord GradeSeparatedJunctionDbaseRecord { get; set; }
        public TransactionZoneDbaseRecord TransactionZoneDbaseRecord { get; set; }
        public TransactionZoneShapeRecord TransactionZoneShapeRecord { get; set; }
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

    public class ZipArchiveDataSet : ZipArchiveIntegrationDataSet
    {
        public List<RoadSegmentEuropeanRoadAttributeDbaseRecord> EuropeanRoadDbaseRecords { get; set; }
        public List<RoadSegmentNationalRoadAttributeDbaseRecord> NationalRoadDbaseRecords { get; set; }
        public List<GradeSeparatedJunctionDbaseRecord> GradeSeparatedJunctionDbaseRecords { get; set; }
        public List<TransactionZoneDbaseRecord> TransactionZoneDbaseRecords { get; set; }
        public List<TransactionZoneShapeRecord> TransactionZoneShapeRecords { get; set; }

        public override void Clear()
        {
            base.Clear();

            EuropeanRoadDbaseRecords.Clear();
            NationalRoadDbaseRecords.Clear();
            GradeSeparatedJunctionDbaseRecords.Clear();
        }

        public void RemoveRoadSegment(int id)
        {
            var roadSegmentDbaseRecordIndex = RoadSegmentDbaseRecords.FindIndex(x => x.WS_OIDN.Value == id);
            RoadSegmentDbaseRecords.RemoveAt(roadSegmentDbaseRecordIndex);
            RoadSegmentShapeRecords.RemoveAt(roadSegmentDbaseRecordIndex);
            EuropeanRoadDbaseRecords.RemoveAll(x => x.WS_TEMPID.Value == id);
            NationalRoadDbaseRecords.RemoveAll(x => x.WS_TEMPID.Value == id);
            GradeSeparatedJunctionDbaseRecords.RemoveAll(x => x.BO_TEMPID.Value == id || x.ON_TEMPID.Value == id);
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

    public class ZipArchiveDataSetStreams : ZipArchiveIntegrationDataSetStreams
    {
        public ZipArchiveDataSetStreams(Fixture fixture, ZipArchiveDataSet set)
            : base(fixture, set)
        {
            EuropeanRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, set.EuropeanRoadDbaseRecords ?? []);
            NationalRoadDbaseRecords = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, set.NationalRoadDbaseRecords ?? []);
            GradeSeparatedJunctionDbaseRecords = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, set.GradeSeparatedJunctionDbaseRecords ?? []);
            TransactionZoneDbaseRecords = fixture.CreateDbfFile(TransactionZoneDbaseRecord.Schema, set.TransactionZoneDbaseRecords ?? []);
            TransactionZoneShapeRecords = fixture.CreateTransactionZoneShapeFile((set.TransactionZoneShapeRecords ?? new List<TransactionZoneShapeRecord>()).Select(x => x.Geometry.ToShapeContent()).ToList());
        }

        public MemoryStream EuropeanRoadDbaseRecords { get; }
        public MemoryStream NationalRoadDbaseRecords { get; }
        public MemoryStream GradeSeparatedJunctionDbaseRecords { get; }
        public MemoryStream TransactionZoneDbaseRecords { get; }
        public MemoryStream TransactionZoneShapeRecords { get; }
    }
}
