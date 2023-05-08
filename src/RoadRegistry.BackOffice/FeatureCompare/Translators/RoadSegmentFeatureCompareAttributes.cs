namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using NetTopologySuite.Geometries;

public class RoadSegmentFeatureCompareAttributes
{
    public int B_WK_OIDN { get; init; }
    public string BEHEER { get; init; }
    public int E_WK_OIDN { get; init; }
    public int? LSTRNMID { get; init; }
    public int METHODE { get; init; }
    public int MORF { get; init; }
    public int? RSTRNMID { get; init; }
    public int STATUS { get; init; }
    public int TGBEP { get; init; }
    public string WEGCAT { get; init; }
    public int WS_OIDN { get; set; }

    public MultiLineString Geometry { get; set; }
}
