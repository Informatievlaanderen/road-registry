namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord
{
    public new static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentEuropeanRoadAttributeDbaseRecord()
    {
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            EU_OIDN,
            WS_OIDN,
            EUNUMMER,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
}
