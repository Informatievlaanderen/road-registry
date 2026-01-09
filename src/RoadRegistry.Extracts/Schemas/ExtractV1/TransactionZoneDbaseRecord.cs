namespace RoadRegistry.Extracts.Schemas.ExtractV1;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class TransactionZoneDbaseRecord : DbaseRecord
{
    public static readonly TransactionZoneDbaseSchema Schema = new();

    public TransactionZoneDbaseRecord()
    {
        SOURCEID = new DbaseInt32(Schema.SOURCEID);
        TYPE = new DbaseInt32(Schema.TYPE);
        BESCHRIJV = new TrimmedDbaseString(Schema.BESCHRIJV);
        OPERATOR = new TrimmedDbaseString(Schema.OPERATOR);
        ORG = new TrimmedDbaseString(Schema.ORG);
        APPLICATIE = new TrimmedDbaseString(Schema.APPLICATIE);
        DOWNLOADID = new TrimmedDbaseString(Schema.DOWNLOADID);

        Values = new DbaseFieldValue[]
        {
            SOURCEID, TYPE, BESCHRIJV, OPERATOR, ORG, APPLICATIE, DOWNLOADID
        };
    }

    public DbaseString APPLICATIE { get; }
    public DbaseString BESCHRIJV { get; }
    public DbaseString DOWNLOADID { get; }
    public DbaseString OPERATOR { get; }
    public DbaseString ORG { get; }
    public DbaseInt32 SOURCEID { get; }
    public DbaseInt32 TYPE { get; }
}
