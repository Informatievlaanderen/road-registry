using System;
using System.IO;
using System.Text;
using Shaperon;
using Shaperon.IO;

namespace Usage
{
    partial class Program
    {
        static void Main(string[] args)
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
                        var record = DbaseRecord.Read(reader, header);
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
                    while (reader.BaseStream.Position < fileByteLength)
                    {
                        var record = ShapeRecord.Read(reader);
                        record.Write(shpWriter);
                        var indexRecord = record.AtOffset(offset);
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
