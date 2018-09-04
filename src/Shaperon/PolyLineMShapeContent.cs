namespace Shaperon
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    public class PolyLineMShapeContent : ShapeContent
    {
        private static readonly ByteLength MeasureRangeByteLength = ByteLength.Doubles("Measure.Min", "Measure.Max");
        private static readonly ByteLength BoundingBoxByteLength = ByteLength.Doubles("BoundingBox.MinX", "BoundingBox.MinY", "BoundingBox.MaxX", "BoundingBox.MaxY");

        public MultiLineString Shape { get; }

        public PolyLineMShapeContent(MultiLineString shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ShapeType = ShapeType.PolyLineM;

            var partCount = shape.NumGeometries;
            var pointCount = shape.NumPoints;
            var contenLength = ByteLength.Int32("ShapeType")
                        .Plus(BoundingBoxByteLength)
                        .Plus(ByteLength.Int32s("NumberOfParts", "NumberOfPoints"))
                        .Plus(ByteLength.Int32("Part").Times(partCount))
                        .Plus(ByteLength.Doubles("Point.X", "Point.Y").Times(pointCount))
                        .ToWordLength();

            ContentLength = GetMeasures().Any(MeasureHasValue)
                ? contenLength
                    .Plus(MeasureRangeByteLength)
                    .Plus(ByteLength.Double("Point.M").Times(pointCount))
                : contenLength;
        }

        private double[] GetMeasures()
        {
            return Shape.GetOrdinates(Ordinate.M);
        }

        private static bool MeasureHasValue(double measure)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator : .Equals() yields different result
            return measure != Coordinate.NullOrdinate;
        }

        private static void SkipMeasureRange(BinaryReader reader)
        {
            reader.ReadBytes(MeasureRangeByteLength);
        }

        private static void SkipBoundingBox(BinaryReader reader)
        {
            reader.ReadBytes(BoundingBoxByteLength);
        }

        internal static ShapeContent ReadFromRecord(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            SkipBoundingBox(reader);
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
                SkipMeasureRange(reader);
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    points[measureIndex].M = reader.ReadDoubleLittleEndian();
                }
            }
            var lines = new ILineString[numParts];
            var toPointIndex = points.Length;
            for(var partIndex = numParts - 1; partIndex >= 0; partIndex--)
            {
                var fromPointIndex = parts[partIndex];
                lines[partIndex] = new LineString(
                    new PointSequence(new ArraySegment<Point>(points, fromPointIndex, toPointIndex - fromPointIndex)),
                    GeometryConfiguration.GeometryFactory
                );
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

            SkipBoundingBox(reader);
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
                SkipMeasureRange(reader);
                for(var measureIndex = 0; measureIndex < numPoints; measureIndex++)
                {
                    points[measureIndex].M = reader.ReadDoubleLittleEndian();
                }
            } //else try-catch-EndOfStreamException?? or only support seekable streams?
            var lines = new ILineString[numParts];
            var toPointIndex = points.Length;
            for(var partIndex = numParts - 1; partIndex >= 0; partIndex--)
            {
                var fromPointIndex = parts[partIndex];
                lines[partIndex] = new LineString(
                    new PointSequence(new ArraySegment<Point>(points, fromPointIndex, toPointIndex - fromPointIndex)),
                    GeometryConfiguration.GeometryFactory
                );
                toPointIndex = fromPointIndex;
            }
            return new PolyLineMShapeContent(new MultiLineString(lines));
        }


        public static ShapeContent FromBytes(byte[] bytes)
        {
            using(var input = new MemoryStream(bytes))
            using(var reader = new BinaryReader(input))
            {
                return Read(reader);
            }
        }

        public static ShapeContent FromBytes(byte[] bytes, Encoding encoding)
        {
            using(var input = new MemoryStream(bytes))
            using(var reader = new BinaryReader(input, encoding))
            {
                return Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            //TODO: If the shape is empty, emit null shape instead?

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type

            var bbox = Shape.EnvelopeInternal;
            writer.WriteDoubleLittleEndian(bbox.MinX);
            writer.WriteDoubleLittleEndian(bbox.MinY);
            writer.WriteDoubleLittleEndian(bbox.MaxX);
            writer.WriteDoubleLittleEndian(bbox.MaxY);
            // num parts
            writer.WriteInt32LittleEndian(Shape.NumGeometries);
            // num points
            writer.WriteInt32LittleEndian(Shape.NumPoints);
            // parts
            var offset = 0;
            foreach(var line in Shape.Geometries.Cast<LineString>())
            {
                writer.WriteInt32LittleEndian(offset);
                offset += line.NumPoints;
            }
            //points
            foreach(var point in Shape.Geometries.Cast<LineString>().SelectMany(line => line.Coordinates))
            {
                writer.WriteDoubleLittleEndian(point.X);
                writer.WriteDoubleLittleEndian(point.Y);
            }
            //has measures?
            var measures = GetMeasures();
            if(measures.Any(MeasureHasValue))
            {
                // measure range
                writer.WriteDoubleLittleEndian(measures.Min());
                writer.WriteDoubleLittleEndian(measures.Max());

                // measures
                foreach(var measure in measures)
                {
                    writer.WriteDoubleLittleEndian(measure);
                }
            }
        }
    }
}
