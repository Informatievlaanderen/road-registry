// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class SurfaceTypeDbaseRecord : DbaseRecord
{
    public SurfaceTypeDbaseRecord()
    {
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        DEFTYPE = new DbaseString(Schema.DEFTYPE);

        Values = new DbaseFieldValue[]
        {
            TYPE, LBLTYPE, DEFTYPE
        };
    }

    public DbaseString DEFTYPE { get; }
    public DbaseString LBLTYPE { get; }
    public static readonly SurfaceTypeDbaseSchema Schema = new();

    public DbaseInt32 TYPE { get; }
}
