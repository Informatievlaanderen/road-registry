namespace RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionDbaseRecord : DbaseRecord
{
    public static readonly GradeSeparatedJunctionDbaseSchema Schema = new();

    public GradeSeparatedJunctionDbaseRecord()
    {
        OK_OIDN = new DbaseInt32(Schema.OK_OIDN);
        BO_TEMPID = new DbaseInt32(Schema.BO_TEMPID);
        ON_TEMPID = new DbaseInt32(Schema.ON_TEMPID);
        TYPE = new DbaseInt32(Schema.TYPE);
        CREATIE = new DbaseDateTime(Schema.CREATIE);

        Values =
        [
            OK_OIDN,
            BO_TEMPID,
            ON_TEMPID,
            TYPE,
            CREATIE
        ];
    }

    public DbaseInt32 OK_OIDN { get; }
    public DbaseInt32 BO_TEMPID { get; }
    public DbaseInt32 ON_TEMPID { get; }
    public DbaseInt32 TYPE { get; }
    public DbaseDateTime CREATIE { get; }
}
