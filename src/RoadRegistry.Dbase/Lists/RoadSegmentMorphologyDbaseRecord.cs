// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
{
    public RoadSegmentMorphologyDbaseRecord()
    {
        MORF = new DbaseInt32(Schema.MORF);
        LBLMORF = new DbaseString(Schema.LBLMORF);
        DEFMORF = new DbaseString(Schema.DEFMORF);

        Values = new DbaseFieldValue[]
        {
            MORF, LBLMORF, DEFMORF
        };
    }

    public DbaseString DEFMORF { get; }
    public DbaseString LBLMORF { get; }

    public DbaseInt32 MORF { get; }
    public static readonly RoadSegmentMorphologyDbaseSchema Schema = new();
}