// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV1.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentMorphologyDbaseSchema Schema = new();

    public RoadSegmentMorphologyDbaseRecord()
    {
        MORF = new DbaseInt32(Schema.MORF);
        LBLMORF = new TrimmedDbaseString(Schema.LBLMORF);
        DEFMORF = new TrimmedDbaseString(Schema.DEFMORF);

        Values = new DbaseFieldValue[]
        {
            MORF, LBLMORF, DEFMORF
        };
    }

    public DbaseString DEFMORF { get; }
    public DbaseString LBLMORF { get; }
    public DbaseInt32 MORF { get; }
}
