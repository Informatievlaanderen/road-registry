// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Extracts.DbaseV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new();

    public RoadSegmentAccessRestrictionDbaseRecord()
    {
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new TrimmedDbaseString(Schema.LBLTYPE);
        DEFTYPE = new TrimmedDbaseString(Schema.DEFTYPE);

        Values = new DbaseFieldValue[]
        {
            TYPE, LBLTYPE, DEFTYPE
        };
    }

    public DbaseString DEFTYPE { get; }
    public DbaseString LBLTYPE { get; }
    public DbaseInt32 TYPE { get; }
}