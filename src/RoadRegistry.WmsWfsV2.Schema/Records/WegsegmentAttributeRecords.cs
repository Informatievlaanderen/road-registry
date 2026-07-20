namespace RoadRegistry.WmsWfsV2.Schema.Records;

using System.Collections.Generic;

// The dynamic (position-varying) attributes of a road segment. These are internal staging only — they are not a product
// output and are never queried directly; they exist so the projection can re-flatten AfgeleideWegsegmenten after a
// partial change. They are therefore stored as a single JSON blob on RoadSegmentRecord (see DynamicAttributes) instead
// of in per-attribute tables. Each list entry is a value valid over [VANPOS, TOTPOS] (and, for sided attributes, a KANT).
public sealed class RoadSegmentDynamicAttributes
{
    public List<RoadSegmentMorphologyAttributeRecord> Morphology { get; set; } = [];
    public List<RoadSegmentCategoryAttributeRecord> Category { get; set; } = [];
    public List<RoadSegmentAccessRestrictionAttributeRecord> AccessRestriction { get; set; } = [];
    public List<RoadSegmentSurfaceTypeAttributeRecord> SurfaceType { get; set; } = [];
    public List<RoadSegmentStreetNameAttributeRecord> StreetName { get; set; } = [];
    public List<RoadSegmentMaintenanceAuthorityAttributeRecord> MaintenanceAuthority { get; set; } = [];
    public List<RoadSegmentCarTrafficDirectionAttributeRecord> CarTrafficDirection { get; set; } = [];
    public List<RoadSegmentBikeTrafficDirectionAttributeRecord> BikeTrafficDirection { get; set; } = [];
    public List<RoadSegmentPedestrianTrafficDirectionAttributeRecord> PedestrianTrafficDirection { get; set; } = [];
}

public sealed class RoadSegmentMorphologyAttributeRecord
{
    public int? MORF { get; set; }
    public string? LBLMORF { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentCategoryAttributeRecord
{
    public string? WEGCAT { get; set; }
    public string? LBLWEGCAT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentAccessRestrictionAttributeRecord
{
    public int? TOEGANG { get; set; }
    public string? LBLTOEGANG { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentSurfaceTypeAttributeRecord
{
    public int? VERHARDING { get; set; }
    public string? LBLVERHARD { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentStreetNameAttributeRecord
{
    public int? STRTNMID { get; set; }
    public int? KANT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentMaintenanceAuthorityAttributeRecord
{
    public string? BEHEER { get; set; }
    public int? KANT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentCarTrafficDirectionAttributeRecord
{
    public int? RICHTING { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentBikeTrafficDirectionAttributeRecord
{
    public int? RICHTING { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public sealed class RoadSegmentPedestrianTrafficDirectionAttributeRecord
{
    public int? RICHTING { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}
