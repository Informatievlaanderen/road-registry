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

            var numberOfParts = shape.NumGeometries;
            var numberOfPoints = shape.NumPoints;

            ContentLength = ByteLength.Int32("ShapeType")
                .Plus(BoundingBoxByteLength)
                .Plus(ByteLength.Int32s("NumberOfParts", "NumberOfPoints"))
                .Plus(ByteLength.Int32("Part").Times(numberOfParts))
                .Plus(ByteLength.Doubles("Point.X", "Point.Y").Times(numberOfPoints))
                .Plus(ByteLength.Double("Point.M").Times(numberOfPoints))
                .ToWordLength();
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

            return Read(
                reader,
                (_,  contentLengthWithoutMeasures) => header.ContentLength > contentLengthWithoutMeasures
            );
        }

        public static ShapeContent Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var typeOfShapeValue = reader.ReadInt32LittleEndian("Type of shape");
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShapeValue))
                throw new ShapeRecordContentException("The Shape Type field does not contain a known type of shape.");

            var shapeType = (ShapeType)typeOfShapeValue;
            if (shapeType == ShapeType.NullShape)
                return NullShapeContent.Instance;
            if(shapeType != ShapeType.PolyLineM)
                throw new ShapeRecordContentException("The Shape Type field does not indicate a PolyLineM shape.");

            return Read(
                reader,
                // assume seekable stream, keep reading while position not at end of the stream
                (streamReader, _) => streamReader.BaseStream.CanSeek && streamReader.BaseStream.Position != streamReader.BaseStream.Length
            );
        }

        private static ShapeContent Read(BinaryReader reader, Func<BinaryReader, ByteLength, bool> canReadMeasures)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            SkipBoundingBox(reader);
            var numberOfParts = reader.ReadInt32LittleEndian("NumberOfParts");
            var numberOfPoints = reader.ReadInt32LittleEndian("NumberOfPoints");
            var parts = new Int32[numberOfParts];
            for (var partIndex = 0; partIndex < numberOfParts; partIndex++)
            {
                parts[partIndex] = reader.ReadInt32LittleEndian($"Part[{partIndex}]");
            }
            var points = new MeasuredPoint[numberOfPoints];
            for (var pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                points[pointIndex] = new MeasuredPoint(
                    reader.ReadDoubleLittleEndian($"Points[{pointIndex}].X"),
                    reader.ReadDoubleLittleEndian($"Points[{pointIndex}].Y")
                );
            }

            var contentLengthWithoutMeasures = new ByteLength(44)
                .Plus(ByteLength.Int32("Parts").Times(numberOfParts))
                .Plus(ByteLength.Doubles("Point.X", "Point.Y").Times(numberOfPoints));

            if (canReadMeasures(reader, contentLengthWithoutMeasures))
            {
                SkipMeasureRange(reader);
                for (var measureIndex = 0; measureIndex < numberOfPoints; measureIndex++)
                {
                    points[measureIndex].ChangeMeasurement(reader.ReadDoubleLittleEndian($"Points[{measureIndex}].M"));
                }
            }
            var lines = new ILineString[numberOfParts];
            var toPointIndex = points.Length;
            for (var partIndex = numberOfParts - 1; partIndex >= 0; partIndex--)
            {
                var fromPointIndex = parts[partIndex];
                lines[partIndex] = new LineString(
                    new PointSequence(new ArraySegment<MeasuredPoint>(points, fromPointIndex, toPointIndex - fromPointIndex)),
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

            writer.WriteInt32LittleEndian((int)ShapeType, "ShapeType");

            var bbox = Shape.EnvelopeInternal;
            writer.WriteDoubleLittleEndian(bbox.MinX, "BoundingBox.MinX");
            writer.WriteDoubleLittleEndian(bbox.MinY, "BoundingBox.MinY");
            writer.WriteDoubleLittleEndian(bbox.MaxX, "BoundingBox.MaxX");
            writer.WriteDoubleLittleEndian(bbox.MaxY, "BoundingBox.MaxY");
            writer.WriteInt32LittleEndian(Shape.NumGeometries, "NumberOfParts");
            writer.WriteInt32LittleEndian(Shape.NumPoints, "NumberOfPoints");

            var lineStrings = Shape
                .Geometries
                .Cast<LineString>()
                .ToArray();

            var lines = lineStrings
                .Select((line, index) => new
                {
                    Index = index,
                    NumberOfPoints = line.NumPoints
                });
            var offset = 0;
            foreach (var line in lines)
            {
                writer.WriteInt32LittleEndian(offset, $"Part[{line.Index}]");
                offset += line.NumberOfPoints;
            }

            var points = lineStrings
                .SelectMany(line => line.Coordinates)
                .Select((point, index) => new
                {
                    Index = index,
                    point.X,
                    point.Y
                });
            foreach(var point in points)
            {
                writer.WriteDoubleLittleEndian(point.X, $"Points[{point.Index}].X");
                writer.WriteDoubleLittleEndian(point.Y, $"Points[{point.Index}].Y");
            }

            var measures = Shape
                .GetOrdinates(Ordinate.M)
                .Select((measure, index) => new
                {
                    Index = index,
                    Measure = measure
                })
                .ToArray();
            var min = measures.Min(item => item.Measure);
            var max = measures.Max(item => item.Measure);
            writer.WriteDoubleLittleEndian(double.IsNaN(min) ? double.PositiveInfinity : min, "Measure.Min");
            writer.WriteDoubleLittleEndian(double.IsNaN(max) ? double.NegativeInfinity : max, "Measure.Max");

            foreach(var item in measures)
            {
                writer.WriteDoubleLittleEndian(item.Measure, $"Points[{item.Index}].M");
            }
        }
    }
}
