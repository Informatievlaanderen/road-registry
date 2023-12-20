namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.AfterFeatureCompare.V1;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using Validation;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveAfterFeatureCompareValidatorTests
{
    public static IEnumerable<object[]> MissingRequiredFileCases
    {
        get
        {
            var fixture = CreateFixture();

            var roadSegmentShapeChangeStream = new MemoryStream();
            var polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
            var roadSegmentShapeChangeRecord =
                polyLineMShapeContent.RecordAs(fixture.Create<RecordNumber>());
            using (var writer = new ShapeBinaryWriter(
                       new ShapeFileHeader(
                           roadSegmentShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                           ShapeType.PolyLineM,
                           BoundingBox3D.FromGeometry(polyLineMShapeContent.Shape)),
                       new BinaryWriter(
                           roadSegmentShapeChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(roadSegmentShapeChangeRecord);
            }

            var roadSegmentProjectionFormatStream = new MemoryStream();
            using (var writer = new StreamWriter(
                       roadSegmentProjectionFormatStream,
                       Encoding.UTF8,
                       leaveOpen: true))
            {
                writer.Write(ProjectionFormat.BelgeLambert1972);
            }

            var roadSegmentChangeDbaseRecord = fixture.Create<RoadSegmentChangeDbaseRecord>();
            var roadSegmentDbaseChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           RoadSegmentChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           roadSegmentDbaseChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(roadSegmentChangeDbaseRecord);
            }

            var europeanRoadChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           EuropeanRoadChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           europeanRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<EuropeanRoadChangeDbaseRecord>());
            }

            var nationalRoadChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           NationalRoadChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           nationalRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<NationalRoadChangeDbaseRecord>());
            }

            var numberedRoadChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           NumberedRoadChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           numberedRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<NumberedRoadChangeDbaseRecord>());
            }

            var laneChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           RoadSegmentLaneChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           laneChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var laneChangeDbaseRecord = fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
                laneChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                writer.Write(laneChangeDbaseRecord);
            }

            var widthChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           RoadSegmentWidthChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           widthChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var widthChangeDbaseRecord = fixture.Create<RoadSegmentWidthChangeDbaseRecord>();
                widthChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                writer.Write(widthChangeDbaseRecord);
            }

            var surfaceChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           RoadSegmentSurfaceChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           surfaceChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var surfaceChangeDbaseRecord = fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
                surfaceChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                writer.Write(surfaceChangeDbaseRecord);
            }

            var roadNodeShapeChangeStream = new MemoryStream();
            var pointShapeContent = fixture.Create<PointShapeContent>();
            var roadNodeShapeChangeRecord =
                pointShapeContent.RecordAs(fixture.Create<RecordNumber>());
            using (var writer = new ShapeBinaryWriter(
                       new ShapeFileHeader(
                           roadNodeShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                           ShapeType.Point,
                           BoundingBox3D.FromGeometry(pointShapeContent.Shape)),
                       new BinaryWriter(
                           roadNodeShapeChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(roadNodeShapeChangeRecord);
            }

            var roadNodeProjectionFormatStream = new MemoryStream();
            using (var writer = new StreamWriter(
                       roadNodeProjectionFormatStream,
                       Encoding.UTF8,
                       leaveOpen: true))
            {
                writer.Write(ProjectionFormat.BelgeLambert1972);
            }

            var roadNodeDbaseChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           RoadNodeChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           roadNodeDbaseChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<RoadNodeChangeDbaseRecord>());
            }

            var gradeSeparatedJunctionChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           GradeSeparatedJunctionChangeDbaseRecord.Schema),
                       new BinaryWriter(
                           gradeSeparatedJunctionChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>());
            }

            var transactionZoneStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           TransactionZoneDbaseRecord.Schema),
                       new BinaryWriter(
                           transactionZoneStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<TransactionZoneDbaseRecord>());
            }

            var requiredFiles = new[]
            {
                "WEGSEGMENT_ALL.SHP",
                "WEGSEGMENT_ALL.PRJ",
                "WEGSEGMENT_ALL.DBF",
                "WEGKNOOP_ALL.SHP",
                "WEGKNOOP_ALL.PRJ",
                "WEGKNOOP_ALL.DBF",
                "ATTEUROPWEG_ALL.DBF",
                "ATTGENUMWEG_ALL.DBF",
                "ATTNATIONWEG_ALL.DBF",
                "ATTRIJSTROKEN_ALL.DBF",
                "ATTWEGBREEDTE_ALL.DBF",
                "ATTWEGVERHARDING_ALL.DBF",
                "RLTOGKRUISING_ALL.DBF",
                "TRANSACTIEZONES.DBF"
            };

            for (var index = 0; index < requiredFiles.Length; index++)
            {
                roadSegmentShapeChangeStream.Position = 0;
                roadSegmentProjectionFormatStream.Position = 0;
                roadSegmentDbaseChangeStream.Position = 0;
                europeanRoadChangeStream.Position = 0;
                nationalRoadChangeStream.Position = 0;
                numberedRoadChangeStream.Position = 0;
                laneChangeStream.Position = 0;
                widthChangeStream.Position = 0;
                surfaceChangeStream.Position = 0;
                roadNodeShapeChangeStream.Position = 0;
                roadNodeProjectionFormatStream.Position = 0;
                roadNodeDbaseChangeStream.Position = 0;
                gradeSeparatedJunctionChangeStream.Position = 0;
                transactionZoneStream.Position = 0;

                var errors = ZipArchiveProblems.None;
                var archiveStream = new MemoryStream();
                using (var createArchive =
                       new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    foreach (var requiredFile in requiredFiles)
                    {
                        switch (requiredFile)
                        {
                            case "WEGSEGMENT_ALL.SHP":
                                if (requiredFiles[index] == "WEGSEGMENT_ALL.SHP")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT_ALL.SHP");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT_ALL.SHP").Open())
                                    {
                                        roadSegmentShapeChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGSEGMENT_ALL.PRJ":
                                if (requiredFiles[index] == "WEGSEGMENT_ALL.PRJ")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT_ALL.PRJ");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT_ALL.PRJ").Open())
                                    {
                                        roadSegmentProjectionFormatStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGSEGMENT_ALL.DBF":
                                if (requiredFiles[index] == "WEGSEGMENT_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT_ALL.DBF").Open())
                                    {
                                        roadSegmentDbaseChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP_ALL.SHP":
                                if (requiredFiles[index] == "WEGKNOOP_ALL.SHP")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP_ALL.SHP");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP_ALL.SHP").Open())
                                    {
                                        roadNodeShapeChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP_ALL.PRJ":
                                if (requiredFiles[index] == "WEGKNOOP_ALL.PRJ")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP_ALL.PRJ");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP_ALL.PRJ").Open())
                                    {
                                        roadNodeProjectionFormatStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP_ALL.DBF":
                                if (requiredFiles[index] == "WEGKNOOP_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP_ALL.DBF").Open())
                                    {
                                        roadNodeDbaseChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTEUROPWEG_ALL.DBF":
                                if (requiredFiles[index] == "ATTEUROPWEG_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTEUROPWEG_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTEUROPWEG_ALL.DBF").Open())
                                    {
                                        europeanRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTGENUMWEG_ALL.DBF":
                                if (requiredFiles[index] == "ATTGENUMWEG_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTGENUMWEG_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTGENUMWEG_ALL.DBF").Open())
                                    {
                                        numberedRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTNATIONWEG_ALL.DBF":
                                if (requiredFiles[index] == "ATTNATIONWEG_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTNATIONWEG_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTNATIONWEG_ALL.DBF").Open())
                                    {
                                        nationalRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTRIJSTROKEN_ALL.DBF":
                                if (requiredFiles[index] == "ATTRIJSTROKEN_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTRIJSTROKEN_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTRIJSTROKEN_ALL.DBF").Open())
                                    {
                                        laneChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTWEGBREEDTE_ALL.DBF":
                                if (requiredFiles[index] == "ATTWEGBREEDTE_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTWEGBREEDTE_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTWEGBREEDTE_ALL.DBF").Open())
                                    {
                                        widthChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTWEGVERHARDING_ALL.DBF":
                                if (requiredFiles[index] == "ATTWEGVERHARDING_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTWEGVERHARDING_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTWEGVERHARDING_ALL.DBF").Open())
                                    {
                                        surfaceChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "RLTOGKRUISING_ALL.DBF":
                                if (requiredFiles[index] == "RLTOGKRUISING_ALL.DBF")
                                {
                                    errors = errors.RequiredFileMissing("RLTOGKRUISING_ALL.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("RLTOGKRUISING_ALL.DBF").Open())
                                    {
                                        gradeSeparatedJunctionChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "TRANSACTIEZONES.DBF":
                                if (requiredFiles[index] == "TRANSACTIEZONES.DBF")
                                {
                                    errors = errors.RequiredFileMissing("TRANSACTIEZONES.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("TRANSACTIEZONES.DBF").Open())
                                    {
                                        transactionZoneStream.CopyTo(entryStream);
                                    }
                                }

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

        var roadSegmentShapeChangeStream = new MemoryStream();
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       ShapeFileHeader.Length,
                       ShapeType.PolyLineM,
                       BoundingBox3D.Empty),
                   new BinaryWriter(
                       roadSegmentShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new ShapeRecord[0]);
        }

        var roadSegmentProjectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   roadSegmentProjectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(string.Empty);
        }

        var roadSegmentDbaseChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       roadSegmentDbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new RoadSegmentChangeDbaseRecord[0]);
        }

        var europeanRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       EuropeanRoadChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       europeanRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new EuropeanRoadChangeDbaseRecord[0]);
        }

        var nationalRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       NationalRoadChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       nationalRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new NationalRoadChangeDbaseRecord[0]);
        }

        var numberedRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(1),
                       NumberedRoadChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       numberedRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new NumberedRoadChangeDbaseRecord[0]);
        }

        var laneChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentLaneChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       laneChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new RoadSegmentLaneChangeDbaseRecord[0]);
        }

        var widthChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentWidthChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       widthChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new RoadSegmentWidthChangeDbaseRecord[0]);
        }

        var surfaceChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(1),
                       RoadSegmentSurfaceChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       surfaceChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new RoadSegmentSurfaceChangeDbaseRecord[0]);
        }

        var roadNodeShapeChangeStream = new MemoryStream();
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       ShapeFileHeader.Length,
                       ShapeType.Point,
                       BoundingBox3D.Empty),
                   new BinaryWriter(
                       roadNodeShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new ShapeRecord[0]);
        }

        var roadNodeProjectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   roadNodeProjectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(string.Empty);
        }

        var roadNodeDbaseChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadNodeChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       roadNodeDbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new RoadNodeChangeDbaseRecord[0]);
        }

        var gradeSeparatedJunctionChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       GradeSeparatedJunctionChangeDbaseRecord.Schema),
                   new BinaryWriter(
                       gradeSeparatedJunctionChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new GradeSeparatedJunctionChangeDbaseRecord[0]);
        }

        var transactionZoneStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       TransactionZoneDbaseRecord.Schema),
                   new BinaryWriter(
                       transactionZoneStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(new TransactionZoneDbaseRecord[0]);
        }

        roadSegmentShapeChangeStream.Position = 0;
        roadSegmentProjectionFormatStream.Position = 0;
        roadSegmentDbaseChangeStream.Position = 0;
        europeanRoadChangeStream.Position = 0;
        nationalRoadChangeStream.Position = 0;
        numberedRoadChangeStream.Position = 0;
        laneChangeStream.Position = 0;
        widthChangeStream.Position = 0;
        surfaceChangeStream.Position = 0;
        roadNodeShapeChangeStream.Position = 0;
        roadNodeProjectionFormatStream.Position = 0;
        roadNodeDbaseChangeStream.Position = 0;
        gradeSeparatedJunctionChangeStream.Position = 0;
        transactionZoneStream.Position = 0;

        var archiveStream = new MemoryStream();
        using (var createArchive =
               new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            using (var entryStream =
                   createArchive.CreateEntry("TRANSACTIEZONES.DBF").Open())
            {
                transactionZoneStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGKNOOP_ALL.DBF").Open())
            {
                roadNodeDbaseChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGKNOOP_ALL.SHP").Open())
            {
                roadNodeShapeChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGKNOOP_ALL.PRJ").Open())
            {
                roadNodeProjectionFormatStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT_ALL.DBF").Open())
            {
                roadSegmentDbaseChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTRIJSTROKEN_ALL.DBF").Open())
            {
                laneChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTWEGBREEDTE_ALL.DBF").Open())
            {
                widthChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTWEGVERHARDING_ALL.DBF").Open())
            {
                surfaceChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT_ALL.SHP").Open())
            {
                roadSegmentShapeChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT_ALL.PRJ").Open())
            {
                roadSegmentProjectionFormatStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTEUROPWEG_ALL.DBF").Open())
            {
                europeanRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTNATIONWEG_ALL.DBF").Open())
            {
                nationalRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTGENUMWEG_ALL.DBF").Open())
            {
                numberedRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("RLTOGKRUISING_ALL.DBF").Open())
            {
                gradeSeparatedJunctionChangeStream.CopyTo(entryStream);
            }
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

        fixture.Customize<EuropeanRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new EuropeanRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE =
                        { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<NationalRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new NationalRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<NumberedRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new NumberedRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
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

        fixture.Customize<RoadNodeChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)random.Next(1, 5) },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WEGKNOOPID = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
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

        fixture.Customize<RoadSegmentChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)random.Next(1, 5) },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
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
                    LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    TGBEP =
                    {
                        Value = (short)fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                    },
                    EVENTIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentLaneChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentLaneChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    RS_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    AANTAL = { Value = (short)fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                    RICHTING =
                    {
                        Value = (short)fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                    }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentSurfaceChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                })
                .OmitAutoProperties());
        fixture.Customize<RoadSegmentWidthChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentWidthChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WB_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOSITIE = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
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
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveAfterFeatureCompareValidator(null));
    }

    [Fact]
    public void IsZipArchiveAfterFeatureCompareValidator()
    {
        var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

        Assert.IsAssignableFrom<IZipArchiveAfterFeatureCompareValidator>(sut);
    }

    [Fact(Skip = "Use me to validate a specific file")]
    public void ValidateActualFile()
    {
        using (var fileStream = File.OpenRead(@""))
        using (var archive = new ZipArchive(fileStream))
        {
            var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

            result.Count.Should().Be(0);
        }
    }

    [Fact]
    public void ValidateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty)));
    }

    [Fact]
    public void ValidateMetadataCanNotBeNull()
    {
        var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

        using (var ms = new MemoryStream())
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create))
        {
            Assert.Throws<ArgumentNullException>(() => sut.Validate(archive, null));
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultFromEntryValidators()
    {
        using (var archive = CreateArchiveWithEmptyFiles())
        {
            var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

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
                            return entry.HasNoDbaseRecords();
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
            var sut = new ZipArchiveAfterFeatureCompareValidator(FileEncoding.UTF8);

            var result = sut.Validate(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty));

            Assert.Equal(expected, result);
        }
    }
}
