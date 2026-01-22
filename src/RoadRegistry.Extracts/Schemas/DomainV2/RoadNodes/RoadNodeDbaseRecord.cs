namespace RoadRegistry.Extracts.Schemas.DomainV2.RoadNodes;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeDbaseSchema Schema = new();

    public RoadNodeDbaseRecord()
    {
        WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        GRENSKNOOP = new DbaseInt16(Schema.GRENSKNOOP);
        CREATIE = new DbaseDateTime(Schema.CREATIE);
        VERSIE = new DbaseDateTime(Schema.VERSIE);

        Values =
        [
            WK_OIDN, TYPE, GRENSKNOOP, CREATIE, VERSIE
        ];
    }

    public DbaseInt32 WK_OIDN { get; }
    public DbaseInt32 TYPE { get; }
    public DbaseInt16 GRENSKNOOP { get; }
    public DbaseDateTime CREATIE { get; }
    public DbaseDateTime VERSIE { get; }
}
