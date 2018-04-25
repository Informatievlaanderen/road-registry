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
            using(var outFile = File.OpenWrite(args[1]))
            using(var inFile = File.OpenRead(args[0]))
            {
                using(var writer = new BinaryWriter(outFile))
                using(var reader = new BinaryReader(inFile))
                {
                    var header = ShapeFileHeader.Read(reader);
                    header.Write(writer);

                    var fileByteLength = header.FileWordLength.ToByteLength();
                    while(reader.BaseStream.Position < fileByteLength)
                    {
                        var record = ShapeFileRecord.Read(reader);
                        record.WriteToShp(writer);
                    }
                    
                    outFile.Flush();
                }
            }
        }
    }
}
