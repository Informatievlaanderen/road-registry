namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Extracts.Dbase.GradeSeparatedJuntions;
using Extracts.Dbase.RoadNodes;
using Extracts.Dbase.RoadSegments;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using Validation;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveBeforeFeatureCompareValidatorTests
{
    public static IEnumerable<object[]> MissingRequiredFileCases
    {
        get
        {
            var fixture = CreateFixture();

            var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFileWithOneRecord();
            var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();
            var roadSegmentChangeDbaseRecord = fixture.Create<RoadSegmentDbaseRecord>();
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
                });
            var widthChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentWidthAttributeDbaseRecord>(
                RoadSegmentWidthAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                });
            var surfaceChangeStream = fixture.CreateDbfFileWithOneRecord<RoadSegmentSurfaceAttributeDbaseRecord>(
                RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                });

            var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFileWithOneRecord();
            var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();
            var roadNodeDbaseChangeStream = fixture.CreateDbfFileWithOneRecord<RoadNodeDbaseRecord>(
                RoadNodeDbaseRecord.Schema);

            var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFileWithOneRecord<GradeSeparatedJunctionDbaseRecord>(
                GradeSeparatedJunctionDbaseRecord.Schema);

            var transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

            var requiredFiles = new[]
            {
                "TRANSACTIEZONES.DBF",
                "EWEGKNOOP.DBF",
                "WEGKNOOP.DBF",
                "EWEGKNOOP.SHP",
                "WEGKNOOP.SHP",
                "WEGKNOOP.PRJ",
                "EWEGSEGMENT.DBF",
                "WEGSEGMENT.DBF",
                "EWEGSEGMENT.SHP",
                "WEGSEGMENT.SHP",
                "WEGSEGMENT.PRJ",
                "EATTRIJSTROKEN.DBF",
                "ATTRIJSTROKEN.DBF",
                "EATTWEGBREEDTE.DBF",
                "ATTWEGBREEDTE.DBF",
                "EATTWEGVERHARDING.DBF",
                "ATTWEGVERHARDING.DBF",
                "EATTEUROPWEG.DBF",
                "ATTEUROPWEG.DBF",
                "EATTNATIONWEG.DBF",
                "ATTNATIONWEG.DBF",
                "EATTGENUMWEG.DBF",
                "ATTGENUMWEG.DBF",
                "ERLTOGKRUISING.DBF",
                "RLTOGKRUISING.DBF"
            };

            for (var index = 0; index < requiredFiles.Length; index++)
            {
                var errors = ZipArchiveProblems.None;
                var archiveStream = new MemoryStream();
                using (var createArchive =
                       new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    void CreateEntryOrRequiredFileMissingError(string file, MemoryStream fileStream)
                    {
                        if (requiredFiles[index] == file)
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

                    foreach (var requiredFile in requiredFiles)
                    {
                        switch (requiredFile)
                        {
                            case "EWEGSEGMENT.SHP":
                            case "WEGSEGMENT.SHP":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentShapeChangeStream);
                                break;
                            case "WEGSEGMENT.PRJ":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentProjectionFormatStream);
                                break;
                            case "EWEGSEGMENT.DBF":
                            case "WEGSEGMENT.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadSegmentDbaseChangeStream);
                                break;
                            case "EWEGKNOOP.SHP":
                            case "WEGKNOOP.SHP":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeShapeChangeStream);
                                break;
                            case "WEGKNOOP.PRJ":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeProjectionFormatStream);
                                break;
                            case "EWEGKNOOP.DBF":
                            case "WEGKNOOP.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, roadNodeDbaseChangeStream);
                                break;
                            case "EATTEUROPWEG.DBF":
                            case "ATTEUROPWEG.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, europeanRoadChangeStream);
                                break;
                            case "EATTGENUMWEG.DBF":
                            case "ATTGENUMWEG.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, numberedRoadChangeStream);
                                break;
                            case "EATTNATIONWEG.DBF":
                            case "ATTNATIONWEG.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, nationalRoadChangeStream);
                                break;
                            case "EATTRIJSTROKEN.DBF":
                            case "ATTRIJSTROKEN.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, laneChangeStream);
                                break;
                            case "EATTWEGBREEDTE.DBF":
                            case "ATTWEGBREEDTE.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, widthChangeStream);
                                break;
                            case "EATTWEGVERHARDING.DBF":
                            case "ATTWEGVERHARDING.DBF":
                                CreateEntryOrRequiredFileMissingError(requiredFile, surfaceChangeStream);
                                break;
                            case "ERLTOGKRUISING.DBF":
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



    private static ZipArchive CreateArchiveWithEmptyFiles()
    {
        var fixture = CreateFixture();

        var roadSegmentShapeChangeStream = fixture.CreateEmptyRoadSegmentShapeFile();
        var roadSegmentProjectionFormatStream = fixture.CreateEmptyProjectionFormatFile();
        var roadSegmentDbaseChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema);

        var europeanRoadChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
        var nationalRoadChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
            RoadSegmentNationalRoadAttributeDbaseRecord.Schema);
        var numberedRoadChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
            RoadSegmentNumberedRoadAttributeDbaseRecord.Schema);
        var laneChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentLaneAttributeDbaseRecord>(
            RoadSegmentLaneAttributeDbaseRecord.Schema);
        var widthChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentWidthAttributeDbaseRecord>(
            RoadSegmentWidthAttributeDbaseRecord.Schema);
        var surfaceChangeStream = fixture.CreateEmptyDbfFile<RoadSegmentSurfaceAttributeDbaseRecord>(
            RoadSegmentSurfaceAttributeDbaseRecord.Schema);

        var roadNodeShapeChangeStream = fixture.CreateEmptyRoadNodeShapeFile();
        var roadNodeProjectionFormatStream = fixture.CreateEmptyProjectionFormatFile();
        var roadNodeDbaseChangeStream = fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(
            RoadNodeDbaseRecord.Schema);

        var gradeSeparatedJunctionChangeStream = fixture.CreateEmptyDbfFile<GradeSeparatedJunctionDbaseRecord>(
            GradeSeparatedJunctionDbaseRecord.Schema);

        var transactionZoneStream = fixture.CreateEmptyDbfFile<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

        var archiveStream = new MemoryStream();
        using (var createArchive =
               new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            void CreateEntryInArchive(string file, MemoryStream fileStream)
            {
                fileStream.Position = 0;

                using (var entryStream = createArchive.CreateEntry(file).Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            CreateEntryInArchive("TRANSACTIEZONES.DBF", transactionZoneStream);
            CreateEntryInArchive("EWEGKNOOP.DBF", roadNodeDbaseChangeStream);
            CreateEntryInArchive("WEGKNOOP.DBF", roadNodeDbaseChangeStream);
            CreateEntryInArchive("WEGKNOOP.SHP", roadNodeShapeChangeStream);
            CreateEntryInArchive("EWEGKNOOP.SHP", roadNodeShapeChangeStream);
            CreateEntryInArchive("WEGKNOOP.PRJ", roadNodeProjectionFormatStream);
            CreateEntryInArchive("EWEGSEGMENT.DBF", roadSegmentDbaseChangeStream);
            CreateEntryInArchive("WEGSEGMENT.DBF", roadSegmentDbaseChangeStream);
            CreateEntryInArchive("EWEGSEGMENT.SHP", roadSegmentShapeChangeStream);
            CreateEntryInArchive("WEGSEGMENT.SHP", roadSegmentShapeChangeStream);
            CreateEntryInArchive("WEGSEGMENT.PRJ", roadSegmentProjectionFormatStream);
            CreateEntryInArchive("EATTRIJSTROKEN.DBF", laneChangeStream);
            CreateEntryInArchive("ATTRIJSTROKEN.DBF", laneChangeStream);
            CreateEntryInArchive("EATTWEGBREEDTE.DBF", widthChangeStream);
            CreateEntryInArchive("ATTWEGBREEDTE.DBF", widthChangeStream);
            CreateEntryInArchive("EATTWEGVERHARDING.DBF", surfaceChangeStream);
            CreateEntryInArchive("ATTWEGVERHARDING.DBF", surfaceChangeStream);
            CreateEntryInArchive("EATTEUROPWEG.DBF", europeanRoadChangeStream);
            CreateEntryInArchive("ATTEUROPWEG.DBF", europeanRoadChangeStream);
            CreateEntryInArchive("EATTNATIONWEG.DBF", nationalRoadChangeStream);
            CreateEntryInArchive("ATTNATIONWEG.DBF", nationalRoadChangeStream);
            CreateEntryInArchive("EATTGENUMWEG.DBF", numberedRoadChangeStream);
            CreateEntryInArchive("ATTGENUMWEG.DBF", numberedRoadChangeStream);
            CreateEntryInArchive("ERLTOGKRUISING.DBF", gradeSeparatedJunctionChangeStream);
            CreateEntryInArchive("RLTOGKRUISING.DBF", gradeSeparatedJunctionChangeStream);
        }

        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
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
                            new CoordinateM(1.0, 1.0, 0)
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

            var result = sut.Validate(archive, ZipArchiveMetadata.Empty);

            result.Count.Should().Be(0);
        }
    }

    [Fact]
    public void ValidateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, ZipArchiveMetadata.Empty));
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
        var filesWithWarning = new[]
        {
            "TRANSACTIEZONES.DBF",
            "EATTEUROPWEG.DBF",
            "ATTEUROPWEG.DBF",
            "EATTNATIONWEG.DBF",
            "ATTNATIONWEG.DBF",
            "EATTGENUMWEG.DBF",
            "ATTGENUMWEG.DBF",
            "ERLTOGKRUISING.DBF",
            "RLTOGKRUISING.DBF"
        };

        using (var archive = CreateArchiveWithEmptyFiles())
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, ZipArchiveMetadata.Empty);
            var expected = ZipArchiveProblems.Many(
                archive.Entries.Select
                (entry =>
                {
                    var extension = Path.GetExtension(entry.Name);
                    switch (extension)
                    {
                        case ".SHP":
                            return entry.HasNoShapeRecords();
                        case ".DBF":
                            return entry.HasNoDbaseRecords(filesWithWarning.Contains(entry.Name));
                        case ".PRJ":
                            return entry.ProjectionFormatInvalid();
                    }

                    return null;
                })
            );

            var files = archive.Entries.Select(x => x.Name).ToArray();

            foreach (var file in files)
            {
                var expectedProblem = expected.Single(x => x.File == file);
                var actualProblem = result.Single(x => x.File == file);
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

            var result = sut.Validate(archive, ZipArchiveMetadata.Empty);

            Assert.Equal(expected, result);
        }
    }
}
