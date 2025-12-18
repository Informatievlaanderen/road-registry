// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV1.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class NumberedRoadSegmentDirectionDbaseRecord : DbaseRecord
{
    public static readonly NumberedRoadSegmentDirectionDbaseSchema Schema = new();

    public NumberedRoadSegmentDirectionDbaseRecord()
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
