using System;
using System.IO;
using Shaperon;
using Shaperon.IO;

namespace Usage
{
    class Program
    {
        static void Main(string[] args)
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
                        var record = ShapeIndexFileRecord.Read(reader);
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
                    var offset = Offset.Initial;
                    while (reader.BaseStream.Position < fileByteLength)
                    {
                        var record = ShapeFileRecord.Read(reader);
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
