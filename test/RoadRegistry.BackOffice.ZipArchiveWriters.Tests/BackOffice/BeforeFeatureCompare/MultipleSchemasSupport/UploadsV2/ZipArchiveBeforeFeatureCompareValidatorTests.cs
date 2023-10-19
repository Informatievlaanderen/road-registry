namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.MultipleSchemasSupport.UploadsV2;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;
using RoadRegistry.Tests.BackOffice;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveBeforeFeatureCompareValidatorTests
{
    public static IEnumerable<object[]> MissingRequiredFileCases
    {
        get
        {
            var fixture = CreateFixture();

            var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();
            var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { fixture.Create<PointShapeContent>(), fixture.Create<PointShapeContent>() });
            var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
            var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
            var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2 });

            var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();
            var roadSegmentPolyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
            var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFileWithOneRecord(roadSegmentPolyLineMShapeContent);
            var roadSegmentChangeDbaseRecord = fixture.Create<RoadSegmentDbaseRecord>();
            roadSegmentChangeDbaseRecord.B_WK_OIDN.Value = roadNodeDbaseRecord1.WK_OIDN.Value;
            roadSegmentChangeDbaseRecord.E_WK_OIDN.Value = roadNodeDbaseRecord2.WK_OIDN.Value;
            var roadSegmentDbaseChangeStream = fixture.CreateDbfFileWithOneRecord(RoadSegmentDbaseRecord.Schema, roadSegmentChangeDbaseRecord);

            var europeanRoadChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
            var nationalRoadChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentNationalRoadAttributeDbaseRecord>(
                RoadSegmentNationalRoadAttributeDbaseRecord.Schema);
            var numberedRoadChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                RoadSegmentNumberedRoadAttributeDbaseRecord.Schema);
            var laneChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentLaneAttributeDbaseRecord>(
                RoadSegmentLaneAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = 0;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.Points.Last().X;
                });
            var widthChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentWidthAttributeDbaseRecord>(
                RoadSegmentWidthAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = 0;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.Points.Last().X;
                });
            var surfaceChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentSurfaceAttributeDbaseRecord>(
                RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = 0;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.Points.Last().X;
                });
            
            var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
            gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
            gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
            var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFileWithOneRecord(GradeSeparatedJunctionDbaseRecord.Schema, gradeSeparatedJunctionDbaseRecord);

            var transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

            var requiredFiles = new[]
            {
                "TRANSACTIEZONES.DBF",
                "WEGKNOOP.DBF",
                "WEGKNOOP.SHP",
                "WEGKNOOP.PRJ",
                "WEGSEGMENT.DBF",
                "WEGSEGMENT.SHP",
                "WEGSEGMENT.PRJ",
                "ATTRIJSTROKEN.DBF",
                "ATTWEGBREEDTE.DBF",
                "ATTWEGVERHARDING.DBF",
                "ATTEUROPWEG.DBF",
                "ATTNATIONWEG.DBF",
                "ATTGENUMWEG.DBF",
                "RLTOGKRUISING.DBF"
            };

            using (var extractZipArchiveTestData = new ExtractsZipArchiveTestData())
            {
                var extractFiles = extractZipArchiveTestData.ExtractFileNames;

                foreach (var fileToBeMissing in requiredFiles)
                {
                    var errors = ZipArchiveProblems.None;
                    var archiveStream = new MemoryStream();
                    using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        void CreateEntryOrRequiredFileMissingError(string file, MemoryStream fileStream)
                        {
                            if (fileToBeMissing == file)
                            {
                                errors = errors.RequiredFileMissing(file);
                            }
                            else
                            {
                                using (var entryStream = createArchive.CreateEntry(file).Open())
                                {
                                    fileStream.Position = 0;
                                    fileStream.CopyTo(entryStream);
                                }
                            }
                        }

                        foreach (var file in extractFiles)
                        {
                            var zipEntry = extractZipArchiveTestData.ZipArchive.Entries.Single(x => x.Name == file);
                            using (var entryStream = createArchive.CreateEntry(file).Open())
                            using (var extractFileStream = zipEntry.Open())
                            {
                                extractFileStream.CopyTo(entryStream);
                            }
                        }

                        foreach (var requiredFile in requiredFiles)
                        {
                            switch (requiredFile)
                            {
                                case "WEGSEGMENT.SHP":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentShapeChangeStream);
                                    break;
                                case "WEGSEGMENT.PRJ":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentProjectionFormatStream);
                                    break;
                                case "WEGSEGMENT.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentDbaseChangeStream);
                                    if (fileToBeMissing == requiredFile)
                                    {
                                        {
                                            var recordContext = ExtractFileName.AttRijstroken.AtDbaseRecord(FeatureType.Change, RecordNumber.Initial);
                                            errors += recordContext.RoadSegmentMissing(roadSegmentChangeDbaseRecord.WS_OIDN.Value);
                                        }
                                        {
                                            var recordContext = ExtractFileName.AttWegbreedte.AtDbaseRecord(FeatureType.Change, RecordNumber.Initial);
                                            errors += recordContext.RoadSegmentMissing(roadSegmentChangeDbaseRecord.WS_OIDN.Value);
                                        }
                                        {
                                            var recordContext = ExtractFileName.AttWegverharding.AtDbaseRecord(FeatureType.Change, RecordNumber.Initial);
                                            errors += recordContext.RoadSegmentMissing(roadSegmentChangeDbaseRecord.WS_OIDN.Value);
                                        }
                                    }
                                    break;
                                case "WEGKNOOP.SHP":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeShapeChangeStream);
                                    break;
                                case "WEGKNOOP.PRJ":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeProjectionFormatStream);
                                    break;
                                case "WEGKNOOP.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeDbaseChangeStream);
                                    if (fileToBeMissing == requiredFile)
                                    {
                                        {
                                            var recordContext = ExtractFileName.Wegsegment.AtDbaseRecord(FeatureType.Change, RecordNumber.Initial);
                                            errors += recordContext.RoadSegmentStartNodeMissing(roadSegmentChangeDbaseRecord.B_WK_OIDN.Value);
                                            errors += recordContext.RoadSegmentEndNodeMissing(roadSegmentChangeDbaseRecord.E_WK_OIDN.Value);
                                        }
                                    }
                                    break;
                                case "ATTEUROPWEG.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, europeanRoadChangeStream);
                                    break;
                                case "ATTGENUMWEG.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, numberedRoadChangeStream);
                                    break;
                                case "ATTNATIONWEG.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, nationalRoadChangeStream);
                                    break;
                                case "ATTRIJSTROKEN.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, laneChangeStream);
                                    break;
                                case "ATTWEGBREEDTE.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, widthChangeStream);
                                    break;
                                case "ATTWEGVERHARDING.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, surfaceChangeStream);
                                    break;
                                case "RLTOGKRUISING.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, gradeSeparatedJunctionChangeStream);
                                    break;
                                case "TRANSACTIEZONES.DBF":
                                    CreateEntryOrRequiredFileMissingError(requiredFile, transactionZoneStream);
                                    break;
                            }
                        }
                    }

                    archiveStream.Position = 0;

                    yield return new object[]
                    {
                        new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8),
                        errors
                    };
                }
            }
        }
    }

    private static ZipArchive CreateArchiveWithEmptyFiles()
    {
        var fixture = CreateFixture();

        var roadSegmentDbaseStream = fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema);

        var europeanRoadStream = fixture.CreateEmptyDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
        var nationalRoadStream = fixture.CreateEmptyDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
            RoadSegmentNationalRoadAttributeDbaseRecord.Schema);
        var numberedRoadStream = fixture.CreateEmptyDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
            RoadSegmentNumberedRoadAttributeDbaseRecord.Schema);
        var laneStream = fixture.CreateEmptyDbfFile<RoadSegmentLaneAttributeDbaseRecord>(
            RoadSegmentLaneAttributeDbaseRecord.Schema);
        var widthStream = fixture.CreateEmptyDbfFile<RoadSegmentWidthAttributeDbaseRecord>(
            RoadSegmentWidthAttributeDbaseRecord.Schema);
        var surfaceStream = fixture.CreateEmptyDbfFile<RoadSegmentSurfaceAttributeDbaseRecord>(
            RoadSegmentSurfaceAttributeDbaseRecord.Schema);

        var roadNodeDbaseStream = fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema);

        var gradeSeparatedJunctionStream = fixture.CreateEmptyDbfFile<GradeSeparatedJunctionDbaseRecord>(
            GradeSeparatedJunctionDbaseRecord.Schema);

        var transactionZoneStream = fixture.CreateEmptyDbfFile<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

        return fixture.CreateUploadZipArchive(new ExtractsZipArchiveTestData(),
            roadSegmentDbaseChangeStream: roadSegmentDbaseStream,
            europeanRoadChangeStream: europeanRoadStream,
            nationalRoadChangeStream: nationalRoadStream,
            numberedRoadChangeStream: numberedRoadStream,
            laneChangeStream: laneStream,
            widthChangeStream: widthStream,
            surfaceChangeStream: surfaceStream,
            roadNodeDbaseChangeStream: roadNodeDbaseStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionStream,
            transactionZoneStream: transactionZoneStream
        );
    }

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.CustomizeRecordType();
        fixture.CustomizeAttributeId();
        fixture.CustomizeRoadSegmentId();
        fixture.CustomizeEuropeanRoadNumber();
        fixture.CustomizeNationalRoadNumber();
        fixture.CustomizeGradeSeparatedJunctionId();
        fixture.CustomizeGradeSeparatedJunctionType();
        fixture.CustomizeNumberedRoadNumber();
        fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        fixture.CustomizeRoadSegmentNumberedRoadDirection();
        fixture.CustomizeRoadNodeId();
        fixture.CustomizeRoadNodeType();
        fixture.CustomizeRoadSegmentGeometryDrawMethod();
        fixture.CustomizeOrganizationId();
        fixture.CustomizeRoadSegmentMorphology();
        fixture.CustomizeRoadSegmentStatus();
        fixture.CustomizeRoadSegmentCategory();
        fixture.CustomizeRoadSegmentAccessRestriction();
        fixture.CustomizeRoadSegmentLaneCount();
        fixture.CustomizeRoadSegmentLaneDirection();
        fixture.CustomizeRoadSegmentPosition();
        fixture.CustomizeRoadSegmentSurfaceType();
        fixture.CustomizeRoadSegmentPosition();
        fixture.CustomizeRoadSegmentWidth();
        fixture.CustomizeRoadSegmentPosition();
        fixture.CustomizeOrganizationId();
        fixture.CustomizeOperatorName();
        fixture.CustomizeReason();
        fixture.CustomizeDownloadId();

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
                    WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<Point>(customization =>
            customization.FromFactory(generator =>
                new Point(
                    fixture.Create<double>(),
                    fixture.Create<double>()
                )
            ).OmitAutoProperties()
        );

        fixture.Customize<RecordNumber>(customizer =>
            customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));

        fixture.Customize<PointShapeContent>(customization =>
            customization
                .FromFactory(random => new PointShapeContent(
                    GeometryTranslator.FromGeometryPoint(fixture.Create<Point>())))
                .OmitAutoProperties()
        );

        fixture.Customize<LineString>(customization =>
            customization.FromFactory(generator =>
                new LineString(
                    new CoordinateArraySequence(
                        new Coordinate[]
                        {
                            new CoordinateM(0.0, 0.0, 0),
                            new CoordinateM(5.0, 0.0, 5.0)
                        }),
                    GeometryConfiguration.GeometryFactory
                )
            ).OmitAutoProperties()
        );

        fixture.Customize<MultiLineString>(customization =>
            customization.FromFactory(generator =>
                new MultiLineString(new[] { fixture.Create<LineString>() })
            ).OmitAutoProperties()
        );
        fixture.Customize<PolyLineMShapeContent>(customization =>
            customization
                .FromFactory(random => new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(fixture.Create<MultiLineString>()))
                )
                .OmitAutoProperties()
        );

        fixture.Customize<RoadSegmentDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                    METHODE =
                    {
                        Value = (short)fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                    },
                    BEHEER = { Value = fixture.Create<OrganizationId>() },
                    MORF =
                        { Value = (short)fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                    STATUS = { Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                    WEGCAT = { Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier },
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

        fixture.Customize<TransactionZoneDbaseRecord>(
            composer => composer
                .FromFactory(random => new TransactionZoneDbaseRecord
                {
                    SOURCEID = { Value = random.Next(1, 5) },
                    TYPE = { Value = random.Next(1, 9999) },
                    BESCHRIJV = { Value = fixture.Create<Reason>().ToString() },
                    DOWNLOADID = { Value = fixture.Create<DownloadId>().ToString() },
                    OPERATOR = { Value = fixture.Create<OperatorName>().ToString() },
                    ORG = { Value = fixture.Create<OrganizationId>().ToString() },
                    APPLICATIE =
                    {
                        Value = new string(fixture
                            .CreateMany<char>(TransactionZoneDbaseRecord.Schema.APPLICATIE.Length.ToInt32())
                            .ToArray())
                    }
                })
                .OmitAutoProperties());
        return fixture;
    }

    [Fact]
    public void EncodingCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveBeforeFeatureCompareValidator(null));
    }

    [Fact]
    public void IsZipArchiveBeforeFeatureCompareValidator()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

        Assert.IsAssignableFrom<IZipArchiveBeforeFeatureCompareValidator>(sut);
    }

    [Fact(Skip = "Use me to validate a specific file")]
    public void ValidateActualFile()
    {
        using (var fileStream = File.OpenRead(@""))
        using (var archive = new ZipArchive(fileStream))
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

            result.Count.Should().Be(0);
        }
    }

    [Fact]
    public void ValidateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty)));
    }

    [Fact]
    public void ValidateMetadataCanNotBeNull()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

        using (var ms = new MemoryStream())
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create))
        {
            Assert.Throws<ArgumentNullException>(() => sut.Validate(archive, null));
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultFromEntryValidators()
    {
        var integrationFiles = new[]
        {
            "IWEGKNOOP.DBF",
            "IWEGKNOOP.SHP",
            "IWEGKNOOP.PRJ",
            "IWEGSEGMENT.DBF",
            "IWEGSEGMENT.SHP",
            "IWEGSEGMENT.PRJ"
        };

        var hasNoDbaseRecordsAsErrorFiles = new[] { "TRANSACTIEZONES.DBF" };

        using (var archive = CreateArchiveWithEmptyFiles())
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));
            var entries = archive.Entries
                .Where(entry => !integrationFiles.Contains(entry.Name, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();

            var expected = ZipArchiveProblems.Many(
                entries.Select
                (entry =>
                {
                    var extension = Path.GetExtension(entry.Name);
                    switch (extension)
                    {
                        case ".SHP":
                            return entry.HasNoShapeRecords();
                        case ".DBF":
                            return entry.HasNoDbaseRecords(hasNoDbaseRecordsAsErrorFiles.Contains(entry.Name));
                        case ".PRJ":
                            return entry.ProjectionFormatInvalid();
                    }

                    return null;
                })
            );

            var files = entries.Select(x => x.Name).ToArray();

            foreach (var file in files)
            {
                var expectedProblem = expected.SingleOrDefault(x => x.File == file);
                if (expectedProblem is null)
                {
                    throw new Exception($"No expected problem found for file {file}");
                }
                var actualProblem = result.SingleOrDefault(x => x.File == file);
                if (actualProblem is null)
                {
                    throw new Exception($"No actual problem found for file {file}");
                }
                Assert.Equal(expectedProblem, actualProblem);
            }
        }
    }

    [Theory]
    [MemberData(nameof(MissingRequiredFileCases))]
    public void ValidateReturnsExpectedResultWhenRequiredFileMissing(ZipArchive archive, ZipArchiveProblems expected)
    {
        using (archive)
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

            Assert.Equal(expected, result);
        }
    }
}
