// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV1.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentCategoryDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentCategoryDbaseSchema Schema = new();

    public RoadSegmentCategoryDbaseRecord()
    {
        WEGCAT = new TrimmedDbaseString(Schema.WEGCAT);
        LBLWEGCAT = new TrimmedDbaseString(Schema.LBLWEGCAT);
        DEFWEGCAT = new TrimmedDbaseString(Schema.DEFWEGCAT);

        Values = new DbaseFieldValue[]
        {
            WEGCAT, LBLWEGCAT, DEFWEGCAT
        };
    }

    public DbaseString DEFWEGCAT { get; }
    public DbaseString LBLWEGCAT { get; }
    public DbaseString WEGCAT { get; }
}
