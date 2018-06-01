using System;
using System.IO;
using System.Linq;
using Wkx;

namespace Shaperon
{
    public class PolyLineMShapeContent : ShapeContent
    {
        public PolyLineMShapeContent(MultiLineString shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ShapeType = ShapeType.PolyLineM;
            var partCount = shape.Geometries.Count;
            var pointCount = shape.Geometries.Aggregate(0, (current, line) => line.Points.Count);
            ContentLength = shape.Dimension == Dimension.Xym
                ? new ByteLength(44 + (4 * partCount) + (16 * pointCount) + 16 + (8 * pointCount)).ToWordLength()
                : new ByteLength(44 + (4 * partCount) + (16 * pointCount)).ToWordLength();
        }
        public MultiLineString Shape { get; }

        internal static ShapeContent ReadFromRecord(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            reader.ReadBytes(4 * 8); // skip bounding box
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
            if(header.ContentLength > requiredContentByteLength)
            {
                reader.ReadBytes(2 * 8); // skip measure range
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    points[measureIndex] = points[measureIndex].WithMeasure(reader.ReadDoubleLittleEndian());
                }
            }
            var lines = new LineString[numParts];
            var toPointIndex = points.Length;
            for(var partIndex = numParts - 1; partIndex >= 0; partIndex--)
            {
                var fromPointIndex = parts[partIndex];
                lines[partIndex] = new LineString(new ArraySegment<Point>(points, fromPointIndex, toPointIndex - fromPointIndex));
                toPointIndex = fromPointIndex;
            }
            return new PolyLineMShapeContent(new MultiLineString(lines));
        }

        public static ShapeContent Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeRecordContentException("The Shape Type field does not contain a known type of shape.");
            if(((ShapeType)typeOfShape) == ShapeType.NullShape)
                return NullShapeContent.Instance;
            if(((ShapeType)typeOfShape) != ShapeType.PolyLineM)
                throw new ShapeRecordContentException("The Shape Type field does not indicate a PolyLineM shape.");

            reader.ReadBytes(4 * 8); // skip bounding box
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
            if(reader.BaseStream.CanSeek && reader.BaseStream.Position != reader.BaseStream.Length)
            {
                reader.ReadBytes(2 * 8); // skip measure range
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    points[measureIndex] = points[measureIndex].WithMeasure(reader.ReadDoubleLittleEndian());
                }
            } //else try-catch-EndOfStreamException?? or only support seekable streams?
            var lines = new LineString[numParts];
            var toPointIndex = points.Length;
            for(var partIndex = numParts - 1; partIndex >= 0; partIndex--)
            {
                var fromPointIndex = parts[partIndex];
                lines[partIndex] = new LineString(new ArraySegment<Point>(points, fromPointIndex, toPointIndex - fromPointIndex));
                toPointIndex = fromPointIndex;
            }
            return new PolyLineMShapeContent(new MultiLineString(lines));
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            //TODO: If the shape is empty, emit null shape instead?

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type

            var bbox = Shape.GetBoundingBox();
            writer.WriteDoubleLittleEndian(bbox.XMin);
            writer.WriteDoubleLittleEndian(bbox.YMin);
            writer.WriteDoubleLittleEndian(bbox.XMax);
            writer.WriteDoubleLittleEndian(bbox.YMax);
            // num parts
            writer.WriteInt32LittleEndian(Shape.Geometries.Count);
            // num points
            writer.WriteInt32LittleEndian(Shape.Geometries.Aggregate(0, (current, line) => line.Points.Count));
            // parts
            var offset = 0;
            foreach(var line in Shape.Geometries)
            {
                writer.WriteInt32LittleEndian(offset);
                offset += line.Points.Count;
            }
            //points
            foreach(var point in Shape.Geometries.SelectMany(line => line.Points))
            {
                writer.WriteDoubleLittleEndian(point.X.Value);
                writer.WriteDoubleLittleEndian(point.Y.Value);
            }
            //has measures?
            if(Shape.Dimension == Dimension.Xym)
            {
                // measure range
                writer.WriteDoubleLittleEndian(Shape.Geometries.Min(line => line.Points.Min(point => point.M.Value)));
                writer.WriteDoubleLittleEndian(Shape.Geometries.Max(line => line.Points.Max(point => point.M.Value)));

                // measures
                foreach(var measure in Shape.Geometries.SelectMany(line => line.Points).Select(point => point.M.Value))
                {
                    writer.WriteDoubleLittleEndian(measure);
                }
            }
        }
    }
}
