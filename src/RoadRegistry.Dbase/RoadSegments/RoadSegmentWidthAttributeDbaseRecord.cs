namespace RoadRegistry.Dbase.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentWidthAttributeDbaseRecord
{
    public new static readonly RoadSegmentWidthAttributeDbaseSchema Schema = new();

    public RoadSegmentWidthAttributeDbaseRecord()
    {
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            WS_GIDN,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }
    
    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseString WS_GIDN { get; set; }
}
