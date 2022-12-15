namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Dbase.GradeSeparatedJuntions;
using Dbase.RoadNodes;
using Dbase.RoadSegments;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Uploads.Schema.V2;
using Validation;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveBeforeFeatureCompareValidatorTests
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

            var roadSegmentChangeDbaseRecord = fixture.Create<Dbase.RoadSegments.RoadSegmentDbaseRecord>();
            var roadSegmentDbaseChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentDbaseRecord.Schema),
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
                           Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           europeanRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord>());
            }

            var nationalRoadChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           nationalRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord>());
            }

            var numberedRoadChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           numberedRoadChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord>());
            }

            var laneChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           laneChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var laneChangeDbaseRecord = fixture.Create<Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord>();
                laneChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                writer.Write(laneChangeDbaseRecord);
            }

            var widthChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           widthChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var widthChangeDbaseRecord = fixture.Create<Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord>();
                widthChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                writer.Write(widthChangeDbaseRecord);
            }

            var surfaceChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord.Schema),
                       new BinaryWriter(
                           surfaceChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                var surfaceChangeDbaseRecord = fixture.Create<Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord>();
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
                           Dbase.RoadNodes.RoadNodeDbaseRecord.Schema),
                       new BinaryWriter(
                           roadNodeDbaseChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<Dbase.RoadNodes.RoadNodeDbaseRecord>());
            }

            var gradeSeparatedJunctionChangeStream = new MemoryStream();
            using (var writer = new DbaseBinaryWriter(
                       new DbaseFileHeader(
                           fixture.Create<DateTime>(),
                           DbaseCodePage.Western_European_ANSI,
                           new DbaseRecordCount(1),
                           Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord.Schema),
                       new BinaryWriter(
                           gradeSeparatedJunctionChangeStream,
                           Encoding.UTF8,
                           true)))
            {
                writer.Write(fixture.Create<Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord>());
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
                            case "WEGSEGMENT.SHP":
                                if (requiredFiles[index] == "WEGSEGMENT.SHP")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT.SHP");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT.SHP").Open())
                                    {
                                        roadSegmentShapeChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGSEGMENT.PRJ":
                                if (requiredFiles[index] == "WEGSEGMENT.PRJ")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT.PRJ");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT.PRJ").Open())
                                    {
                                        roadSegmentProjectionFormatStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGSEGMENT.DBF":
                                if (requiredFiles[index] == "WEGSEGMENT.DBF")
                                {
                                    errors = errors.RequiredFileMissing("WEGSEGMENT.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGSEGMENT.DBF").Open())
                                    {
                                        roadSegmentDbaseChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP.SHP":
                                if (requiredFiles[index] == "WEGKNOOP.SHP")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP.SHP");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP.SHP").Open())
                                    {
                                        roadNodeShapeChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP.PRJ":
                                if (requiredFiles[index] == "WEGKNOOP.PRJ")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP.PRJ");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP.PRJ").Open())
                                    {
                                        roadNodeProjectionFormatStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "WEGKNOOP.DBF":
                                if (requiredFiles[index] == "WEGKNOOP.DBF")
                                {
                                    errors = errors.RequiredFileMissing("WEGKNOOP.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("WEGKNOOP.DBF").Open())
                                    {
                                        roadNodeDbaseChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTEUROPWEG.DBF":
                                if (requiredFiles[index] == "ATTEUROPWEG.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTEUROPWEG.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTEUROPWEG.DBF").Open())
                                    {
                                        europeanRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTGENUMWEG.DBF":
                                if (requiredFiles[index] == "ATTGENUMWEG.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTGENUMWEG.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTGENUMWEG.DBF").Open())
                                    {
                                        numberedRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTNATIONWEG.DBF":
                                if (requiredFiles[index] == "ATTNATIONWEG.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTNATIONWEG.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTNATIONWEG.DBF").Open())
                                    {
                                        nationalRoadChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTRIJSTROKEN.DBF":
                                if (requiredFiles[index] == "ATTRIJSTROKEN.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTRIJSTROKEN.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTRIJSTROKEN.DBF").Open())
                                    {
                                        laneChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTWEGBREEDTE.DBF":
                                if (requiredFiles[index] == "ATTWEGBREEDTE.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTWEGBREEDTE.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTWEGBREEDTE.DBF").Open())
                                    {
                                        widthChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "ATTWEGVERHARDING.DBF":
                                if (requiredFiles[index] == "ATTWEGVERHARDING.DBF")
                                {
                                    errors = errors.RequiredFileMissing("ATTWEGVERHARDING.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("ATTWEGVERHARDING.DBF").Open())
                                    {
                                        surfaceChangeStream.CopyTo(entryStream);
                                    }
                                }

                                break;
                            case "RLTOGKRUISING.DBF":
                                if (requiredFiles[index] == "RLTOGKRUISING.DBF")
                                {
                                    errors = errors.RequiredFileMissing("RLTOGKRUISING.DBF");
                                }
                                else
                                {
                                    using (var entryStream =
                                           createArchive.CreateEntry("RLTOGKRUISING.DBF").Open())
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
                       RoadSegmentDbaseRecord.Schema),
                   new BinaryWriter(
                       roadSegmentDbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentDbaseRecord>());
        }

        var europeanRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       europeanRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentEuropeanRoadAttributeDbaseRecord>());
        }

        var nationalRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentNationalRoadAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       nationalRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentNationalRoadAttributeDbaseRecord>());
        }

        var numberedRoadChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(1),
                       RoadSegmentNumberedRoadAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       numberedRoadChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentNumberedRoadAttributeDbaseRecord>());
        }

        var laneChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentLaneAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       laneChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentLaneAttributeDbaseRecord>());
        }

        var widthChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       RoadSegmentWidthAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       widthChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentWidthAttributeDbaseRecord>());
        }

        var surfaceChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(1),
                       RoadSegmentSurfaceAttributeDbaseRecord.Schema),
                   new BinaryWriter(
                       surfaceChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadSegmentSurfaceAttributeDbaseRecord>());
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
            writer.Write(Array.Empty<ShapeRecord>());
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
                       RoadNodeDbaseRecord.Schema),
                   new BinaryWriter(
                       roadNodeDbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<RoadNodeDbaseRecord>());
        }

        var gradeSeparatedJunctionChangeStream = new MemoryStream();
        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       GradeSeparatedJunctionDbaseRecord.Schema),
                   new BinaryWriter(
                       gradeSeparatedJunctionChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<GradeSeparatedJunctionDbaseRecord>());
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
            writer.Write(Array.Empty<TransactionZoneDbaseRecord>());
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
                   createArchive.CreateEntry("WEGKNOOP.DBF").Open())
            {
                roadNodeDbaseChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGKNOOP.SHP").Open())
            {
                roadNodeShapeChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGKNOOP.PRJ").Open())
            {
                roadNodeProjectionFormatStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT.DBF").Open())
            {
                roadSegmentDbaseChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT.SHP").Open())
            {
                roadSegmentShapeChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("WEGSEGMENT.PRJ").Open())
            {
                roadSegmentProjectionFormatStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTRIJSTROKEN.DBF").Open())
            {
                laneChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTWEGBREEDTE.DBF").Open())
            {
                widthChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTWEGVERHARDING.DBF").Open())
            {
                surfaceChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTEUROPWEG.DBF").Open())
            {
                europeanRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTNATIONWEG.DBF").Open())
            {
                nationalRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("ATTGENUMWEG.DBF").Open())
            {
                numberedRoadChangeStream.CopyTo(entryStream);
            }

            using (var entryStream =
                   createArchive.CreateEntry("RLTOGKRUISING.DBF").Open())
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
        
        fixture.Customize<Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.GradeSeparatedJuntions.GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE =
                        { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentNumberedRoadAttributeDbaseRecord
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

        fixture.Customize<Dbase.RoadNodes.RoadNodeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadNodes.RoadNodeDbaseRecord
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
                        new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(1.0, 1.0)
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

        fixture.Customize<Dbase.RoadSegments.RoadSegmentDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentDbaseRecord
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

        fixture.Customize<Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentLaneAttributeDbaseRecord
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

        fixture.Customize<Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                })
                .OmitAutoProperties());
        fixture.Customize<Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new Dbase.RoadSegments.RoadSegmentWidthAttributeDbaseRecord
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
        var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

        Assert.IsAssignableFrom<IZipArchiveBeforeFeatureCompareValidator>(sut);
    }

    [Fact(Skip = "Use me to validate a specific file")]
    public void ValidateActualFile()
    {
        using (var fileStream = File.OpenRead(@""))
        using (var archive = new ZipArchive(fileStream))
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

            var result = sut.Validate(archive, ZipArchiveMetadata.Empty);

            result.Count.Should().Be(0);
        }
    }

    [Fact]
    public void ValidateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, ZipArchiveMetadata.Empty));
    }

    [Fact]
    public void ValidateMetadataCanNotBeNull()
    {
        var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

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
            var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

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
                            return entry.HasNoDbaseRecords(false);
                        case ".PRJ":
                            return entry.ProjectionFormatInvalid();
                    }

                    return null;
                })
            );
            
            Assert.Equal(expected, result);
        }
    }

    [Theory]
    [MemberData(nameof(MissingRequiredFileCases))]
    public void ValidateReturnsExpectedResultWhenRequiredFileMissing(ZipArchive archive, ZipArchiveProblems expected)
    {
        using (archive)
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8);

            var result = sut.Validate(archive, ZipArchiveMetadata.Empty);

            Assert.Equal(expected, result);
        }
    }
}
