namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNumberedRoadAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord
{
    public new static readonly RoadSegmentNumberedRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNumberedRoadAttributeDbaseRecord()
    {
        LBLRICHT = new DbaseString(Schema.LBLRICHT);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            LBLRICHT,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLRICHT { get; }
}
