// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.Inwinning.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new();

    public RoadSegmentAccessRestrictionDbaseRecord()
    {
        TOEGANG = new DbaseInt32(Schema.TOEGANG);
        LBLTOEGANG = new TrimmedDbaseString(Schema.LBLTOEGANG);
        DEFTOEGANG = new TrimmedDbaseString(Schema.DEFTOEGANG);

        Values =
        [
            TOEGANG, LBLTOEGANG, DEFTOEGANG
        ];
    }

    public DbaseString DEFTOEGANG { get; }
    public DbaseString LBLTOEGANG { get; }
    public DbaseInt32 TOEGANG { get; }
}
