namespace RoadRegistry.Wms.Schema;

using System;

public class RoadSegmentNationalRoadAttributeRecord
{
    public int WS_OIDN { get; set; }
    public string BEGINORG { get; set; }
    public DateTime BEGINTIJD { get; set; }
    public string IDENT2 { get; set; }
    public string LBLBGNORG { get; set; }
    // Set once the road segment it belongs to is ingewonnen (migrated to V2): the view that serves this table leaves it out from then on.
    public bool IsV2 { get; set; }
    public int NW_OIDN { get; set; }
}
