namespace RoadRegistry.Extracts.Schemas.Inwinning;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class TransactionZoneDbaseRecord : DbaseRecord
{
    public static readonly TransactionZoneDbaseSchema Schema = new();

    public TransactionZoneDbaseRecord()
    {
        TYPE = new DbaseInt32(Schema.TYPE);
        BESCHRIJV = new TrimmedDbaseString(Schema.BESCHRIJV);
        DOWNLOADID = new TrimmedDbaseString(Schema.DOWNLOADID);

        Values =
        [
            TYPE, BESCHRIJV, DOWNLOADID
        ];
    }

    public DbaseInt32 TYPE { get; }
    public DbaseString BESCHRIJV { get; }
    public DbaseString DOWNLOADID { get; }
}
