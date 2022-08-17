// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeTypeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeTypeDbaseSchema Schema = new();

    public RoadNodeTypeDbaseRecord()
    {
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        DEFTYPE = new DbaseString(Schema.DEFTYPE);

        Values = new DbaseFieldValue[]
        {
            TYPE, LBLTYPE, DEFTYPE
        };
    }

    public DbaseInt32 TYPE { get; }
    public DbaseString LBLTYPE { get; }
    public DbaseString DEFTYPE { get; }
}
