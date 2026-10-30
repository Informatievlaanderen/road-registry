// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.DomainV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class SurfaceTypeDbaseRecord : DbaseRecord
{
    public static readonly SurfaceTypeDbaseSchema Schema = new();

    public SurfaceTypeDbaseRecord()
    {
        VERHARDING = new DbaseInt32(Schema.VERHARDING);
        LBLVERHARD = new TrimmedDbaseString(Schema.LBLVERHARD);
        DEFVERHARD = new TrimmedDbaseString(Schema.DEFVERHARD);

        Values =
        [
            VERHARDING, LBLVERHARD, DEFVERHARD
        ];
    }

    public DbaseString DEFVERHARD { get; }
    public DbaseString LBLVERHARD { get; }
    public DbaseInt32 VERHARDING { get; }
}
