using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoadRegistry.Projections;
using Shaperon;
using Wkx;

namespace Usage
{
    partial class Program
    {
        private static async Task Main(string[] args)
        {
            // var gem = LineString.Deserialize<WkbSerializer>(new byte[]
            // {
            //     0x01,
            //     0x02,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0x02,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0x00,
            //     0xC6,
            //     0x88,
            //     0x0A,
            //     0x41,
            //     0x80,
            //     0x9B,
            //     0xC4,
            //     0x20,
            //     0x48,
            //     0x2A,
            //     0x06,
            //     0x41,
            //     0x00,
            //     0xAE,
            //     0x47,
            //     0xE1,
            //     0xC0,
            //     0x89,
            //     0x0A,
            //     0x41,
            //     0x80,
            //     0x9B,
            //     0xC4,
            //     0x20,
            //     0xDC,
            //     0x27,
            //     0x06,
            //     0x41,
            // });
            // Console.WriteLine(gem.);
            //ReadWriteShpAndShx(args);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                InitialCatalog = "RoadRegistry",
                IntegratedSecurity = false,
                UserID = "sa",
                Password = "K@rm3l13t"
            };
            var schema = new RoadNodeDbaseSchema();
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseSqlServer(
                    builder.ConnectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                    }).Options;
            var encoding = Encoding.GetEncoding(1252);
            using(var context = new ShapeContext(options))
            {
                // using (var inShpFile = File.OpenRead("roadsegment.shp"))
                // {
                //     using (var shpReader = new BinaryReader(inShpFile, Encoding.ASCII))
                //     {
                //         var shpHeader = ShapeFileHeader.Read(shpReader);
                //         Console.WriteLine("Can read");
                //     }
                // }

                using (var outDbfFile = File.OpenWrite("roadsegment.dbf"))
                using (var outShpFile = File.OpenWrite("roadsegment.shp"))
                using (var outShxFile = File.OpenWrite("roadsegment.shx"))
                {
                    using (var shxWriter = new BinaryWriter(outShxFile, Encoding.ASCII))
                    using (var shpWriter = new BinaryWriter(outShpFile, Encoding.ASCII))
                    using (var dbfWriter = new BinaryWriter(outDbfFile, encoding))
                    {
                        // TODO: Make sure there's a transaction to ensure the count and iteration are in sync
                        var recordCount = await context.RoadSegments.CountAsync();
                        var dbfHeader = new DbaseFileHeader(
                            DateTime.Now,
                            DbaseCodePage.WindowsANSI,
                            new DbaseRecordCount(recordCount),
                            schema
                        );
                        dbfHeader.Write(dbfWriter);
                        //var fileLength = new WordLength(await context.RoadSegments.SumAsync(segment => segment.ShapeRecordContentLength));
                        var shpFileLength = new WordLength(50);
                        var dbfRecord = new RoadSegmentDbaseRecord();
                        var shpRecords = new List<ShapeRecord>();
                        var shxRecords = new List<ShapeIndexRecord>();
                        var envelope = new Envelope();
                        var number = RecordNumber.Initial;
                        var offset = ShapeRecord.InitialOffset;
                        foreach(var segment in context.RoadSegments)
                        {
                            dbfRecord.FromBytes(segment.DbaseRecord, encoding);
                            dbfRecord.Write(dbfWriter);

                            using(var stream = new MemoryStream(segment.ShapeRecordContent))
                            {
                                using(var reader = new BinaryReader(stream))
                                {
                                    switch(PolyLineMShapeContent.Read(reader))
                                    {
                                        case PolyLineMShapeContent content:
                                            envelope.ExpandToInclude(content.Shape.EnvelopeInternal);
                                            var shpRecord1 = content.RecordAs(number);
                                            shpFileLength = shpFileLength.Plus(shpRecord1.Length);
                                            var shxRecord1 = shpRecord1.IndexAt(offset);
                                            shpRecords.Add(shpRecord1);
                                            shxRecords.Add(shxRecord1);
                                            offset = offset.Plus(shpRecord1.Length);
                                            number = number.Next();
                                            break;
                                        case NullShapeContent content:
                                            var shpRecord2 = content.RecordAs(number);
                                            shpFileLength = shpFileLength.Plus(shpRecord2.Length);
                                            var shxRecord2 = shpRecord2.IndexAt(offset);
                                            shpRecords.Add(shpRecord2);
                                            shxRecords.Add(shxRecord2);
                                            offset = offset.Plus(shpRecord2.Length);
                                            number = number.Next();
                                            break;
                                    }

                                    
                                    // Console.WriteLine("File length is " + shpFileLength.ToInt32());
                                    // Console.WriteLine("Offset is " + offset.ToInt32());
                                    // Console.WriteLine("Record number is " + number.ToInt32());
                                    // Console.WriteLine();
                                }
                            }
                        }
                        dbfWriter.Write(DbaseRecord.EndOfFile);
                        outDbfFile.Flush();
                        
                        // Console.WriteLine("File length is " + shpFileLength.ToInt32());
                        var shpHeader = new ShapeFileHeader(shpFileLength, ShapeType.PolyLineM, new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0));
                        shpHeader.Write(shpWriter);
                        var shxHeader = new ShapeFileHeader(new WordLength(50 + 4 * shxRecords.Count), ShapeType.PolyLineM, new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0));
                        shxHeader.Write(shxWriter);

                        foreach(var shpRecord in shpRecords)
                        {
                            shpRecord.Write(shpWriter);
                        }
                        shpWriter.Flush();

                        foreach(var shxRecord in shxRecords)
                        {
                            shxRecord.Write(shxWriter);
                        }
                        shxWriter.Flush();

                        Console.WriteLine("Done");
                    }
                }
            }

        }

        private static void ReadWriteDbf(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var outDbfFile = File.OpenWrite(args[1]))
            using (var inFile = File.OpenRead(args[0]))
            {
                using (var dbfWriter = new BinaryWriter(outDbfFile, Encoding.ASCII)) //Encoding.GetEncoding(1252)
                using (var reader = new BinaryReader(inFile, Encoding.ASCII))
                {
                    var header = DbaseFileHeader.Read(reader);
                    header.Write(dbfWriter);

                    while (reader.BaseStream.Position != reader.BaseStream.Length
                        && reader.PeekChar() != (char)DbaseRecord.EndOfFile)
                    {
                        var record = new AnonymousDbaseRecord(header.Schema.Fields);
                        record.Read(reader);
                        record.Write(dbfWriter);
                    }
                    dbfWriter.Write(DbaseRecord.EndOfFile);
                    outDbfFile.Flush();
                }
            }
        }

        private static void ReadWriteShx(string[] args)
        {
            using (var outShxFile = File.OpenWrite(args[1]))
            using (var inFile = File.OpenRead(args[0]))
            {
                using (var shxWriter = new BinaryWriter(outShxFile))
                using (var reader = new BinaryReader(inFile))
                {
                    var header = ShapeFileHeader.Read(reader);
                    header.Write(shxWriter);

                    var fileByteLength = header.FileLength.ToByteLength();
                    while (reader.BaseStream.Position < fileByteLength)
                    {
                        var record = ShapeIndexRecord.Read(reader);
                        record.Write(shxWriter);
                    }

                    outShxFile.Flush();
                }
            }
        }

        private static void ReadWriteShpAndShx(string[] args)
        {
            using (var outShxFile = File.OpenWrite(args[2]))
            using (var outShpFile = File.OpenWrite(args[1]))
            using (var inFile = File.OpenRead(args[0]))
            {
                using (var shxWriter = new BinaryWriter(outShxFile))
                using (var shpWriter = new BinaryWriter(outShpFile))
                using (var reader = new BinaryReader(inFile))
                {
                    var header = ShapeFileHeader.Read(reader);
                    header.Write(shpWriter);
                    header.Write(shxWriter);

                    var fileByteLength = header.FileLength.ToByteLength();
                    var offset = ShapeRecord.InitialOffset;
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var record = ShapeRecord.Read(reader);
                        record.Write(shpWriter);
                        var indexRecord = record.IndexAt(offset);
                        indexRecord.Write(shxWriter);
                        offset = indexRecord.Offset.Plus(indexRecord.ContentLength);
                    }

                    outShpFile.Flush();
                    outShxFile.Flush();
                }
            }
        }
    }
}
