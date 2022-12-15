namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentNationalRoadAttributeDbaseRecord
{
    public new static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNationalRoadAttributeDbaseRecord()
    {
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
}
