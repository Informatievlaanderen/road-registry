﻿using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Shaperon.IO
{
    public class ShapeFileHeader
    {
        public ShapeFileHeader(WordLength fileWordLength, ShapeType shapeType, BoundingBox3D boundingBox)
        {
            FileLength = fileWordLength;
            ShapeType = shapeType;
            BoundingBox = boundingBox;
        }

        public WordLength FileLength { get; }
        public ShapeType ShapeType { get; }
        public BoundingBox3D BoundingBox { get; }

        public static ShapeFileHeader Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ReadInt32BigEndian() != 9994)
                throw new ShapeFileException("The File Code field does not match 9994.");
            reader.ReadBytes(20);
            var fileLength = reader.ReadInt32BigEndian();
            if(reader.ReadInt32LittleEndian() != 1000)
                throw new ShapeFileException("The Version field does not match 1000.");
            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeFileException("The Shape Type field does not contain a known type of shape.");
            var boundingBox = new BoundingBox3D(
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                //Z
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                //M
                ParseNoData(reader.ReadDoubleLittleEndian()),
                ParseNoData(reader.ReadDoubleLittleEndian())
            );
            return new ShapeFileHeader(new WordLength(fileLength), (ShapeType)typeOfShape, boundingBox);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32BigEndian(9994);
            for(var index = 0; index < 20; index++)
            {
                writer.Write(0x0);
            }
            writer.WriteInt32BigEndian(FileLength);
            writer.WriteInt32LittleEndian(1000);
            writer.WriteInt32LittleEndian((int)ShapeType);
            writer.WriteDoubleLittleEndian(BoundingBox.XMin);
            writer.WriteDoubleLittleEndian(BoundingBox.YMin);
            writer.WriteDoubleLittleEndian(BoundingBox.XMax);
            writer.WriteDoubleLittleEndian(BoundingBox.YMax);
            writer.WriteDoubleLittleEndian(BoundingBox.ZMin);
            writer.WriteDoubleLittleEndian(BoundingBox.ZMax);
            writer.WriteDoubleLittleEndian(EscapeNoData(BoundingBox.MMin));
            writer.WriteDoubleLittleEndian(EscapeNoData(BoundingBox.MMax));
        }

        private static double ParseNoData(double value)
        {
            return value < -10e38 ? Double.NaN : value;
        }

        private static double EscapeNoData(double value)
        {
            return value == Double.NaN ? -10e39 : value;
        }
    }
}
