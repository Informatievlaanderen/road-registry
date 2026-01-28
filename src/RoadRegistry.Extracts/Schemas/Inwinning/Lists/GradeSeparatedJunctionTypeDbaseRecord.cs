// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.Inwinning.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionTypeDbaseRecord : DbaseRecord
{
    public static readonly GradeSeparatedJunctionTypeDbaseSchema Schema = new();

    public GradeSeparatedJunctionTypeDbaseRecord()
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
