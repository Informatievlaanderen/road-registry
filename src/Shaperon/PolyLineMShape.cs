using System;
using System.IO;
using Shaperon.IO;

namespace Shaperon
{
    public class PolyLineMShape : IShape
    {
        public PolyLineMShape(BoundingBox2D boundingBox, Int32[] parts, Point[] points, MeasureRange measureRange, double[] measures)
        {
            BoundingBox = boundingBox ?? throw new ArgumentNullException(nameof(boundingBox));
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));
            Points = points ?? throw new ArgumentNullException(nameof(points));
            MeasureRange = measureRange;
            Measures = measures;
            ContentWordLength = MeasureRange != null && Measures != null
                ? new ByteLength(84 + (4 * Parts.Length) + (16 * Points.Length) + (8 * Points.Length)).ToWordLength()
                : new ByteLength(76 + (4 * Parts.Length) + (16 * Points.Length)).ToWordLength();
        }
        public ShapeType ShapeType => ShapeType.PolyLineM;

        public BoundingBox2D BoundingBox { get; }
        public int[] Parts { get; }
        public Point[] Points { get; }
        public MeasureRange MeasureRange { get; }
        public double[] Measures { get; }
        public WordLength ContentWordLength { get; }

        public static IShape Read(BinaryReader reader, ShapeFileRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var box = new BoundingBox2D(
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian(),
                reader.ReadDoubleLittleEndian()
            );
            var numParts = reader.ReadInt32LittleEndian();
            var numPoints = reader.ReadInt32LittleEndian();
            var parts = new Int32[numParts];
            for(var partIndex = 0; partIndex < numParts; partIndex++)
            {
                parts[partIndex] = reader.ReadInt32LittleEndian();
            }
            var points = new Point[numPoints];
            for(var pointIndex = 0; pointIndex < numPoints; pointIndex++)
            {
                points[pointIndex] = new Point(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()
                );
            }
            var requiredContentByteLength = new ByteLength(44 + (4 * numParts) + (16 * numPoints));
            if(header.ContentWordLength > requiredContentByteLength)
            {
                var measureRange = new MeasureRange(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()
                );
                var measures = new double[numPoints];
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    measures[measureIndex] = reader.ReadDoubleLittleEndian();
                }
                return new PolyLineMShape(box, parts, points, measureRange, measures);
            }
            return new PolyLineMShape(box, parts, points, null, null);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type

            writer.WriteDoubleLittleEndian(BoundingBox.XMin);
            writer.WriteDoubleLittleEndian(BoundingBox.YMin);
            writer.WriteDoubleLittleEndian(BoundingBox.XMax);
            writer.WriteDoubleLittleEndian(BoundingBox.YMax);

            writer.WriteInt32LittleEndian(Parts.Length);
            writer.WriteInt32LittleEndian(Points.Length);
            foreach(var part in Parts)
            {
                writer.WriteInt32LittleEndian(part);
            }
            foreach(var point in Points)
            {
                writer.WriteDoubleLittleEndian(point.X);
                writer.WriteDoubleLittleEndian(point.Y);
            }
            if(MeasureRange != null && Measures != null)
            {
                writer.WriteDoubleLittleEndian(MeasureRange.MMin);
                writer.WriteDoubleLittleEndian(MeasureRange.MMax);

                foreach(var measure in Measures)
                {
                    writer.WriteDoubleLittleEndian(measure);
                }
            }
        }
    }
}