namespace RoadRegistry.Pbs.Projections;

using System.Collections.Generic;
using RoadRegistry.ValueObjects;

// Maps V1 (legacy) domain values to their V2 equivalents for the PBS product, using the same migration tables as the
// Inwinning ZipArchiveWriters (RoadSegmentsZipArchiveWriter / RoadNodesZipArchiveWriter / GradeSeparatedJunctionZipArchiveWriter).
// Where those writers throw for an unmapped value, PBS returns null instead - the product stores null when no mapping was possible.
internal static class V1ToV2
{
    public static RoadSegmentGeometryDrawMethodV2? Method(string v1)
    {
        if (!RoadSegmentGeometryDrawMethod.CanParse(v1))
        {
            return null;
        }

        var method = RoadSegmentGeometryDrawMethod.Parse(v1);
        if (method == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
        }
        if (method == RoadSegmentGeometryDrawMethod.Measured
            || method == RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications)
        {
            return RoadSegmentGeometryDrawMethodV2.Ingemeten;
        }
        return null;
    }

    public static RoadSegmentStatusV2? Status(string v1, RoadSegmentGeometryDrawMethodV2 method)
    {
        if (!RoadSegmentStatus.CanParse(v1))
        {
            return null;
        }

        var status = RoadSegmentStatus.Parse(v1);
        if (status == RoadSegmentStatus.PermitRequested
            || status == RoadSegmentStatus.PermitGranted
            || status == RoadSegmentStatus.UnderConstruction)
        {
            return RoadSegmentStatusV2.Gepland;
        }
        if (status == RoadSegmentStatus.InUse)
        {
            return RoadSegmentStatusV2.Gerealiseerd;
        }
        if (status == RoadSegmentStatus.OutOfUse)
        {
            return RoadSegmentStatusV2.BuitenGebruik;
        }
        if (status == RoadSegmentStatus.Unknown)
        {
            return method == RoadSegmentGeometryDrawMethodV2.Ingemeten
                ? RoadSegmentStatusV2.Gerealiseerd
                : RoadSegmentStatusV2.Gepland;
        }
        return null;
    }

    private static readonly Dictionary<int, int?> MorphologyMapping = new()
    {
        { 101, 1 }, { 102, 2 }, { 103, 3 }, { 104, 7 }, { 105, 3 }, { 106, 3 }, { 107, 5 }, { 108, 5 }, { 109, 4 },
        { 110, 3 }, { 111, 6 }, { 112, 6 }, { 113, null }, { 114, null }, { 116, 11 }, { 120, null }, { 125, 8 },
        { 130, 12 }, { -8, -8 }
    };
    public static RoadSegmentMorphologyV2? Morphology(string v1)
    {
        if (!RoadSegmentMorphology.CanParse(v1))
        {
            return null;
        }
        return Map(RoadSegmentMorphology.Parse(v1).Translation.Identifier, MorphologyMapping, RoadSegmentMorphologyV2.ByIdentifier);
    }

    private static readonly Dictionary<int, int?> AccessRestrictionMapping = new()
    {
        { 1, 10 }, { 2, null }, { 3, null }, { 4, 11 }, { 5, 10 }, { 6, 10 }
    };
    public static RoadSegmentAccessRestrictionV2? AccessRestriction(string v1)
    {
        if (!RoadSegmentAccessRestriction.CanParse(v1))
        {
            return null;
        }
        return Map(RoadSegmentAccessRestriction.Parse(v1).Translation.Identifier, AccessRestrictionMapping, RoadSegmentAccessRestrictionV2.ByIdentifier);
    }

    private static readonly Dictionary<int, int> SurfaceTypeMapping = new()
    {
        { 1, 10 }, { 2, 11 }, { -9, 12 }, { -8, -8 }
    };
    public static RoadSegmentSurfaceTypeV2? SurfaceType(string v1)
    {
        if (!RoadSegmentSurfaceType.CanParse(v1))
        {
            return null;
        }
        return Map(RoadSegmentSurfaceType.Parse(v1).Translation.Identifier, SurfaceTypeMapping, RoadSegmentSurfaceTypeV2.ByIdentifier);
    }

    private static readonly Dictionary<string, string> CategoryMapping = new()
    {
        { "EHW", "EHW" }, { "VHW", "VHW" }, { "RW", "RW" }, { "IW", "IW" }, { "OW", "OW" }, { "EW", "EW" },
        { "-8", "-8" }, { "-9", "-9" },
        // obsolete values, only present on tst/stg
        { "L", "OW" }, { "L1", "EW" }, { "L2", "EW" }, { "L3", "EW" }, { "H", "EHW" }, { "PI", "VHW" }, { "PII", "VHW" },
        { "PII-1", "VHW" }, { "PII-2", "VHW" }, { "PII-3", "VHW" }, { "PII-4", "VHW" }, { "S", "RW" }, { "S1", "IW" },
        { "S2", "IW" }, { "S3", "IW" }, { "S4", "IW" }
    };
    public static RoadSegmentCategoryV2? Category(string v1)
    {
        if (!RoadSegmentCategory.CanParse(v1))
        {
            return null;
        }
        if (!CategoryMapping.TryGetValue(RoadSegmentCategory.Parse(v1).Translation.Identifier, out var v2Identifier))
        {
            return null;
        }
        return RoadSegmentCategoryV2.ByIdentifier.TryGetValue(v2Identifier, out var v2) ? v2 : null;
    }

    private static readonly Dictionary<int, int> RoadNodeTypeMapping = new()
    {
        { 1, 10 }, { 2, 11 }, { 3, 12 }, { 4, 10 }, { 5, 13 }
    };
    public static RoadNodeTypeV2? RoadNodeType(string v1)
    {
        if (!ValueObjects.RoadNodeType.CanParse(v1))
        {
            return null;
        }
        return Map(ValueObjects.RoadNodeType.Parse(v1).Translation.Identifier, RoadNodeTypeMapping, RoadNodeTypeV2.ByIdentifier);
    }

    private static readonly Dictionary<int, int> GradeSeparatedJunctionTypeMapping = new()
    {
        { 1, 1 }, { 2, 2 }, { -8, -8 }
    };
    public static GradeSeparatedJunctionTypeV2? GradeSeparatedJunctionType(string v1)
    {
        if (!ValueObjects.GradeSeparatedJunctionType.CanParse(v1))
        {
            return null;
        }
        return Map(ValueObjects.GradeSeparatedJunctionType.Parse(v1).Translation.Identifier, GradeSeparatedJunctionTypeMapping, GradeSeparatedJunctionTypeV2.ByIdentifier);
    }

    private static TV2? Map<TV2>(int v1Identifier, IReadOnlyDictionary<int, int> mapping, IReadOnlyDictionary<int, TV2> byIdentifier)
        where TV2 : class
    {
        if (!mapping.TryGetValue(v1Identifier, out var v2Identifier))
        {
            return null;
        }
        return byIdentifier.TryGetValue(v2Identifier, out var v2) ? v2 : null;
    }

    private static TV2? Map<TV2>(int v1Identifier, IReadOnlyDictionary<int, int?> mapping, IReadOnlyDictionary<int, TV2> byIdentifier)
        where TV2 : class
    {
        if (!mapping.TryGetValue(v1Identifier, out var v2Identifier) || v2Identifier is null)
        {
            return null;
        }
        return byIdentifier.TryGetValue(v2Identifier.Value, out var v2) ? v2 : null;
    }
}
