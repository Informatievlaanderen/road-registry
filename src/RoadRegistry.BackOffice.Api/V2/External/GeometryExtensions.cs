namespace Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

//TODO-pr remove with dotnet10 upgrade and use nuget Be.Vlaanderen.Basisregisters.GrAr.Common
using System;
using System.Linq;
using System.Text;
using System.Xml;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using SpatialTools.GeometryCoordinates;

public static class GeometryExtensions2
{
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";

    public static string ConvertToGml2(this Geometry geometry)
    {
        return geometry.ConvertToGml2(true);
    }

    public static string ConvertToGml2(this Geometry geometry, bool useHttpsSchema)
    {
        if (geometry is not Polygon && geometry is not MultiPolygon && geometry is not LineString && geometry is not Point)
            throw new InvalidOperationException();

        var builder = new StringBuilder();
        var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };

        if (geometry is Polygon polygon)
        {
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Polygon", GmlNamespace);
                WriteSrsName(xmlwriter, geometry, useHttpsSchema);
                WriteRing((polygon.ExteriorRing as LinearRing)!, xmlwriter);
                WriteInteriorRings(polygon.InteriorRings, polygon.NumInteriorRings, xmlwriter);
                xmlwriter.WriteEndElement();
            }
        }
        else if (geometry is MultiPolygon multiPolygon)
        {
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "MultiSurface", GmlNamespace);
                WriteSrsName(xmlwriter, geometry, useHttpsSchema);

                foreach (var p in multiPolygon.Geometries.Cast<Polygon>())
                {
                    xmlwriter.WriteStartElement("gml", "surfaceMember", null!);
                    xmlwriter.WriteStartElement("gml", "Polygon", null!);

                    WriteRing((p.ExteriorRing as LinearRing)!, xmlwriter);
                    WriteInteriorRings(p.InteriorRings, p.NumInteriorRings, xmlwriter);

                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();
                }

                xmlwriter.WriteEndElement();
            }
        }
        else if (geometry is LineString lineString)
        {
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "LineString", GmlNamespace);
                WriteSrsName(xmlwriter, geometry, useHttpsSchema);
                WritePosList(lineString.Coordinates, xmlwriter);
                xmlwriter.WriteEndElement();
            }
        }
        else if (geometry is Point point)
        {
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Point", GmlNamespace);
                WriteSrsName(xmlwriter, point, useHttpsSchema);

                xmlwriter.WriteStartElement("gml", "pos", null!);
                xmlwriter.WriteValue(string.Format(Global.GetNfi(), "{0} {1}",
                    point.Coordinate.X.ToPointGeometryCoordinateValueFormat(),
                    point.Coordinate.Y.ToPointGeometryCoordinateValueFormat()));
                xmlwriter.WriteEndElement();

                xmlwriter.WriteEndElement();
            }
        }

        return builder.ToString();
    }

    private static void WriteSrsName(XmlWriter xmlWriter, Geometry geometry, bool useHttpsSchema)
    {
        switch (geometry.SRID)
        {
            case SystemReferenceId.SridLambert72:
                xmlWriter.WriteAttributeString("srsName", useHttpsSchema ? SystemReferenceId.SrsNameLambert72.Replace("http://", "https://") : SystemReferenceId.SrsNameLambert72);
                break;
            case SystemReferenceId.SridLambert2008:
                xmlWriter.WriteAttributeString("srsName", useHttpsSchema ? SystemReferenceId.SrsNameLambert2008.Replace("http://", "https://") : SystemReferenceId.SrsNameLambert2008);
                break;
            default:
                throw new InvalidOperationException($"Unsupported SRID: {geometry.SRID}.");
        }
    }

    private static void WriteRing(
        LinearRing ring,
        XmlWriter writer,
        bool isInterior = false)
    {
        writer.WriteStartElement("gml", isInterior ? "interior" : "exterior", GmlNamespace);
        writer.WriteStartElement("gml", "LinearRing", GmlNamespace);

        WritePosList(ring.Coordinates, writer);

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WritePosList(
        Coordinate[] coordinates,
        XmlWriter writer)
    {
        writer.WriteStartElement("gml", "posList", GmlNamespace);

        var posListBuilder = new StringBuilder();
        foreach (var coordinate in coordinates)
        {
            posListBuilder.Append(string.Format(
                Global.GetNfi(),
                "{0} {1} ",
                coordinate.X.ToPolygonGeometryCoordinateValueFormat(),
                coordinate.Y.ToPolygonGeometryCoordinateValueFormat()));
        }

        //remove last space
        posListBuilder.Length--;

        writer.WriteValue(posListBuilder.ToString());

        writer.WriteEndElement();
    }

    private static void WriteInteriorRings(
        LineString[] rings,
        int numInteriorRings,
        XmlWriter writer)
    {
        if (numInteriorRings < 1)
        {
            return;
        }

        foreach (var ring in rings)
        {
            WriteRing((ring as LinearRing)!, writer, true);
        }
    }
}
