// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Dbase.Extracts.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class LaneDirectionDbaseRecord : DbaseRecord
{
    public static readonly LaneDirectionDbaseSchema Schema = new();

    public LaneDirectionDbaseRecord()
    {
        RICHTING = new DbaseInt32(Schema.RICHTING);
        LBLRICHT = new DbaseString(Schema.LBLRICHT);
        DEFRICHT = new DbaseString(Schema.DEFRICHT);

        Values = new DbaseFieldValue[]
        {
            RICHTING, LBLRICHT, DEFRICHT
        };
    }

    public DbaseString DEFRICHT { get; }
    public DbaseString LBLRICHT { get; }
    public DbaseInt32 RICHTING { get; }
}