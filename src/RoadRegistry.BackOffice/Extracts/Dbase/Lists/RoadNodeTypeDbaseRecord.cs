// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Extracts.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeTypeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeTypeDbaseSchema Schema = new();

    public RoadNodeTypeDbaseRecord()
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