// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseRecord : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentDbaseRecord
{
    public new static readonly RoadSegmentDbaseSchema Schema = new();

    public RoadSegmentDbaseRecord()
    {
        WS_UIDN = new DbaseString(Schema.WS_UIDN);
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        LBLSTATUS = new DbaseString(Schema.LBLSTATUS);
        LBLMORF = new DbaseString(Schema.LBLMORF);
        LBLWEGCAT = new DbaseString(Schema.LBLWEGCAT);
        LSTRNM = new DbaseString(Schema.LSTRNM);
        RSTRNM = new DbaseString(Schema.RSTRNM);
        LBLBEHEER = new DbaseString(Schema.LBLBEHEER);
        LBLMETHOD = new DbaseString(Schema.LBLMETHOD);
        OPNDATUM = new DbaseDateTime(Schema.OPNDATUM);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);
        LBLTGBEP = new DbaseString(Schema.LBLTGBEP);

        Values = Values.Concat(new DbaseFieldValue[]
        {
            WS_UIDN,
            WS_GIDN,
            LBLSTATUS,
            LBLMORF,
            LBLWEGCAT,
            LSTRNM,
            RSTRNM,
            LBLBEHEER,
            LBLMETHOD,
            OPNDATUM,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG,
            LBLTGBEP
        }).ToArray();
    }
    
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBEHEER { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLMETHOD { get; }
    public DbaseString LBLMORF { get; }
    public DbaseString LBLSTATUS { get; }
    public DbaseString LBLTGBEP { get; }
    public DbaseString LBLWEGCAT { get; }
    public DbaseString LSTRNM { get; }
    public DbaseDateTime OPNDATUM { get; }
    public DbaseString RSTRNM { get; }
    public DbaseString WS_GIDN { get; }
    public DbaseString WS_UIDN { get; }
}
