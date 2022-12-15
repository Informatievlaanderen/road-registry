namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentSurfaceAttributeDbaseRecord
{
    public new static readonly RoadSegmentSurfaceAttributeDbaseSchema Schema = new();

    public RoadSegmentSurfaceAttributeDbaseRecord()
    {
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            WS_GIDN,
            LBLTYPE,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseString LBLTYPE { get; set; }
    public DbaseString WS_GIDN { get; set; }
}
