// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Extracts.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class LaneDirectionDbaseRecord : DbaseRecord
{
    public static readonly LaneDirectionDbaseSchema Schema = new();

    public LaneDirectionDbaseRecord()
    {
        RICHTING = new DbaseInt32(Schema.RICHTING);
        LBLRICHT = new TrimmedDbaseString(Schema.LBLRICHT);
        DEFRICHT = new TrimmedDbaseString(Schema.DEFRICHT);

        Values = new DbaseFieldValue[]
        {
            RICHTING, LBLRICHT, DEFRICHT
        };
    }

    public DbaseString DEFRICHT { get; }
    public DbaseString LBLRICHT { get; }
    public DbaseInt32 RICHTING { get; }
}