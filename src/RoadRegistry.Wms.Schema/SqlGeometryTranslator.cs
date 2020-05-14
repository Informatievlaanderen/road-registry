namespace RoadRegistry.Wms.Schema
{
    using System;
    using System.Data.SqlTypes;
    using BackOffice.Messages;
    using Microsoft.SqlServer.Types;

    public static class SqlGeometryTranslator
    {
        [Flags]
        private enum SerializationProps : byte
        {
            None = 0,
            HasZ = 1,
            HasM = 2,
            IsValid = 4,
            IsSinglePoint = 8,
            IsSingleLineSegment = 16,
            IsLargerThanAHemisphere = 32
        }

        private const int Lambert1972Srid = 31370;

        private const SerializationProps GeometrySerializationProps =
            SerializationProps.HasZ | SerializationProps.HasM | SerializationProps.IsValid;

        private const SerializationProps Geometry2DSerializationProps =
            SerializationProps.IsValid;

        public static byte[] TranslateToSqlGeometry(RoadSegmentGeometry geometry)
        {
            var builder = new SqlGeometryBuilder();
            builder.SetSrid(Lambert1972Srid);
            builder.BeginGeometry(OpenGisGeometryType.LineString);

            var lineString = geometry.MultiLineString[0];

            builder.BeginFigure(lineString.Points[0].X, lineString.Points[0].Y, 0, lineString.Measures[0]);
            for (int i = 1; i < geometry.MultiLineString[0].Points.Length; i++)
            {
                builder.AddLine(lineString.Points[i].X, lineString.Points[i].Y, 0, lineString.Measures[i]);
            }

            builder.EndFigure();
            builder.EndGeometry();
            var buffer = builder.ConstructedGeometry.Serialize().Buffer;
            buffer[5] =  (byte) GeometrySerializationProps;
            return buffer;
        }

        public static SqlGeometry TranslateGeometry(ImportedRoadSegment importedRoadSegment)
        {
            var sqlGeometry = BuildGeometry(importedRoadSegment);
            var sqlGeometryAsBytes = OverwriteSerializationPropsByte(sqlGeometry, GeometrySerializationProps);
            return SqlGeometry.Deserialize(new SqlBytes(sqlGeometryAsBytes));
        }

        public static SqlGeometry TranslateGeometry2D(ImportedRoadSegment importedRoadSegment)
        {
            var sqlGeometry = BuildGeometry2D(importedRoadSegment);
            var sqlGeometryAsBytes = OverwriteSerializationPropsByte(sqlGeometry, Geometry2DSerializationProps);
            return SqlGeometry.Deserialize(new SqlBytes(sqlGeometryAsBytes));
        }

        private static byte[] OverwriteSerializationPropsByte(SqlGeometry sqlGeometry, SerializationProps flag)
        {
            var buffer = sqlGeometry.Serialize().Buffer;
            var builderConstructedBytes = new byte[buffer.Length];
            buffer.CopyTo(new Memory<byte>(builderConstructedBytes));
            builderConstructedBytes[5] = (byte) flag;
            return builderConstructedBytes;
        }

        private static SqlGeometry BuildGeometry2D(ImportedRoadSegment importedRoadSegment)
        {
            var builder = new SqlGeometryBuilder();
            builder.SetSrid(Lambert1972Srid);
            builder.BeginGeometry(OpenGisGeometryType.LineString);

            var lineString = importedRoadSegment.Geometry.MultiLineString[0];

            builder.BeginFigure(lineString.Points[0].X, lineString.Points[0].Y, null, null);
            for (int i = 1; i < importedRoadSegment.Geometry.MultiLineString[0].Points.Length; i++)
            {
                builder.AddLine(lineString.Points[i].X, lineString.Points[i].Y, null, null);
            }

            builder.EndFigure();
            builder.EndGeometry();
            return builder.ConstructedGeometry;
        }

        private static SqlGeometry BuildGeometry(ImportedRoadSegment importedRoadSegment)
        {
            var builder = new SqlGeometryBuilder();
            builder.SetSrid(Lambert1972Srid);
            builder.BeginGeometry(OpenGisGeometryType.LineString);

            var lineString = importedRoadSegment.Geometry.MultiLineString[0];

            builder.BeginFigure(lineString.Points[0].X, lineString.Points[0].Y, 0, lineString.Measures[0]);
            for (int i = 1; i < importedRoadSegment.Geometry.MultiLineString[0].Points.Length; i++)
            {
                builder.AddLine(lineString.Points[i].X, lineString.Points[i].Y, 0, lineString.Measures[i]);
            }

            builder.EndFigure();
            builder.EndGeometry();
            return builder.ConstructedGeometry;
        }
    }
}
