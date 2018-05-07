using System;
using System.IO;
using System.Linq;
using Shaperon.IO;
using Wkx;

namespace Shaperon
{
    public class PolyLineMShapeContent : IShapeContent
    {
        public PolyLineMShapeContent(MultiLineString shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ContentLength = shape.Dimension == Dimension.Xym
                ? new ByteLength(84 + (4 * 1) + (16 * shape.Points.Count) + (8 * shape.Points.Count)).ToWordLength()
                : new ByteLength(76 + (4 * 1) + (16 * shape.Points.Count)).ToWordLength();
            
        }
        public ShapeType ShapeType => ShapeType.PolyLineM;
        public MultiLineString Shape { get; }
        public WordLength ContentLength { get; }

        public static IShapeContent Read(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            reader.ReadBytes(4 * 8); // skip bounding box
            var numParts = reader.ReadInt32LittleEndian();
            var numPoints = reader.ReadInt32LittleEndian();
            reader.ReadBytes(numParts * 4); // skip parts (perhaps enforce that it needs to be 1).
            for(var partIndex = 0; partIndex < numParts; partIndex++)
            {
                reader.ReadInt32LittleEndian(); //
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
            if(header.ContentLength > requiredContentByteLength)
            {
                var measureRange = new MeasureRange(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()
                );
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    points[measureIndex] = new Point(
                        points[measureIndex].X.Value,
                        points[measureIndex].Y.Value,
                        null,
                        reader.ReadDoubleLittleEndian());
                }
                //return new PolyLineMShapeContent(box, parts, points, measureRange, measures);
            }
            return new PolyLineMShapeContent(new LineString(points));
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type

            var bbox = Shape.GetBoundingBox();
            writer.WriteDoubleLittleEndian(bbox.XMin);
            writer.WriteDoubleLittleEndian(bbox.YMin);
            writer.WriteDoubleLittleEndian(bbox.XMax);
            writer.WriteDoubleLittleEndian(bbox.YMax);

            writer.WriteInt32LittleEndian(Shape.Geometries.Count);
            writer.WriteInt32LittleEndian(Shape.Geometries.Aggregate(0, (current, line) => line.Points.Count));
            var offset = 0;
            foreach(var line in Shape.Geometries)
            {
                writer.WriteInt32LittleEndian(0);
            }
            
            foreach(var point in Shape.Points)
            {
                writer.WriteDoubleLittleEndian(point.X.Value);
                writer.WriteDoubleLittleEndian(point.Y.Value);
            }
            if(Shape.Dimension == Dimension.Xym)
            {
                writer.WriteDoubleLittleEndian(Shape.Points.Min(point => point.M.Value));
                writer.WriteDoubleLittleEndian(Shape.Points.Max(point => point.M.Value));

                foreach(var measure in Shape.Points.Select(point => point.M.Value))
                {
                    writer.WriteDoubleLittleEndian(measure);
                }
            }
        }
    }
}