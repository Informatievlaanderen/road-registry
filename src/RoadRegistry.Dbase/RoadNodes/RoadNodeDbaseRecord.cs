namespace RoadRegistry.Dbase.RoadNodes;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadNodeDbaseRecord
{
    public new static readonly RoadNodeDbaseSchema Schema = new();

    public RoadNodeDbaseRecord()
    {
        WK_UIDN = new DbaseString(Schema.WK_UIDN);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            WK_UIDN, LBLTYPE, BEGINTIJD, BEGINORG, LBLBGNORG
        }).ToArray();
    }

    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLTYPE { get; }
    public DbaseString WK_UIDN { get; }
}
