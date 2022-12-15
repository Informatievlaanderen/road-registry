namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentLaneAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentLaneAttributeDbaseRecord
{
    public new static readonly RoadSegmentLaneAttributeDbaseSchema Schema = new();

    public RoadSegmentLaneAttributeDbaseRecord()
    {
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        LBLRICHT = new DbaseString(Schema.LBLRICHT);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);
        
        Values = Values.Concat(new DbaseFieldValue[]
        {
            WS_GIDN,
            LBLRICHT,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseString LBLRICHT { get; set; }
    public DbaseString WS_GIDN { get; set; }
}
