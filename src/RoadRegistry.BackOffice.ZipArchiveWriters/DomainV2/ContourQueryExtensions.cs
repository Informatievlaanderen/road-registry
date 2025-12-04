namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2;

using Editor.Schema.GradeSeparatedJunctions;
using Editor.Schema.RoadNodes;
using Editor.Schema.RoadSegments;
using Extracts.Dbase.RoadSegments;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

// NOTE: If you change the properties of any of the entities used below, you will need to update these queries too!
public static class ContourQueryExtensions
{
    public static IQueryable<RoadNodeRecord> InsideContour(
        this DbSet<RoadNodeRecord> source, IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [node].[Id], [node].[DbaseRecord], [node].[Geometry], [node].[ShapeRecordContent], [node].[ShapeRecordContentLength], [node].[BoundingBox_MaximumX], [node].[BoundingBox_MaximumY], [node].[BoundingBox_MinimumX], [node].[BoundingBox_MinimumY]
FROM [RoadRegistryEditor].[RoadNode] AS [node]
WHERE [node].[Id] IN (
    SELECT [segment1].[StartNodeId]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment1]
    WHERE [segment1].[IsRemoved] = CAST(0 as bit) AND [segment1].[Geometry].STIntersects(@contour) = CAST(1 AS bit))
OR [node].[Id] IN (
    SELECT [segment2].[EndNodeId]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment2]
    WHERE [segment2].[IsRemoved] = CAST(0 as bit) AND [segment2].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<GradeSeparatedJunctionRecord> InsideContour(
        this DbSet<GradeSeparatedJunctionRecord> source, IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [junction].[Id], [junction].[DbaseRecord], [junction].[UpperRoadSegmentId], [junction].[LowerRoadSegmentId]
FROM [RoadRegistryEditor].[GradeSeparatedJunction] AS [junction]
WHERE [junction].[UpperRoadSegmentId] IN (
    SELECT [segment1].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment1]
    WHERE [segment1].[IsRemoved] = CAST(0 as bit) AND [segment1].[Geometry].STIntersects(@contour) = CAST(1 AS bit))
OR [junction].[LowerRoadSegmentId] IN (
    SELECT [segment2].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment2]
    WHERE [segment2].[IsRemoved] = CAST(0 as bit) AND [segment2].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentRecord> InsideContour(
        this DbSet<RoadSegmentRecord> source, IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [segment].*
FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit)", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentEuropeanRoadAttributeRecord> InsideContour(
        this DbSet<RoadSegmentEuropeanRoadAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentEuropeanRoadAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentNationalRoadAttributeRecord> InsideContour(
        this DbSet<RoadSegmentNationalRoadAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentNationalRoadAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentNumberedRoadAttributeRecord> InsideContour(
        this DbSet<RoadSegmentNumberedRoadAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentNumberedRoadAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentLaneAttributeRecord> InsideContour(
        this DbSet<RoadSegmentLaneAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentLaneAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentWidthAttributeRecord> InsideContour(
        this DbSet<RoadSegmentWidthAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentWidthAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    public static IQueryable<RoadSegmentSurfaceAttributeRecord> InsideContour(
        this DbSet<RoadSegmentSurfaceAttributeRecord> source,
        IPolygonal contour)
    {
        return source.FromSqlRaw(
            @"SELECT [attribute].[Id], [attribute].[RoadSegmentId], [attribute].[DbaseRecord]
FROM [RoadRegistryEditor].[RoadSegmentSurfaceAttribute] AS [attribute]
WHERE [attribute].[RoadSegmentId] IN (
    SELECT [segment].[Id]
    FROM [RoadRegistryEditor].[RoadSegment] AS [segment]
    WHERE [segment].[IsRemoved] = CAST(0 as bit) AND [segment].[Geometry].STIntersects(@contour) = CAST(1 AS bit))", contour.ToSqlParameter());
    }

    private static SqlParameter ToSqlParameter(this IPolygonal contour)
    {
        return ((Geometry)contour).ToSqlParameter("contour");
    }
}
