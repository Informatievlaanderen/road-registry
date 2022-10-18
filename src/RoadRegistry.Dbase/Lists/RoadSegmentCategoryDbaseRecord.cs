// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentCategoryDbaseRecord : DbaseRecord
{
    public RoadSegmentCategoryDbaseRecord()
    {
        WEGCAT = new DbaseString(Schema.WEGCAT);
        LBLWEGCAT = new DbaseString(Schema.LBLWEGCAT);
        DEFWEGCAT = new DbaseString(Schema.DEFWEGCAT);

        Values = new DbaseFieldValue[]
        {
            WEGCAT, LBLWEGCAT, DEFWEGCAT
        };
    }

    public DbaseString DEFWEGCAT { get; }
    public DbaseString LBLWEGCAT { get; }
    public static readonly RoadSegmentCategoryDbaseSchema Schema = new();

    public DbaseString WEGCAT { get; }
}
