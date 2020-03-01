namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class TransactionZoneDbaseRecord : DbaseRecord
    {
        public static readonly TransactionZoneDbaseSchema Schema = new TransactionZoneDbaseSchema();

        public TransactionZoneDbaseRecord()
        {
            SOURCE_ID = new DbaseNumber(Schema.SOURCE_ID);
            TYPE = new DbaseNumber(Schema.TYPE);
            BESCHRIJV = new DbaseCharacter(Schema.BESCHRIJV);
            OPERATOR = new DbaseCharacter(Schema.OPERATOR);
            ORG = new DbaseCharacter(Schema.ORG);
            APPLICATIE = new DbaseCharacter(Schema.APPLICATIE);

            Values = new DbaseFieldValue[]
            {
                SOURCE_ID, TYPE, BESCHRIJV, OPERATOR, ORG, APPLICATIE
            };
        }

        public DbaseNumber SOURCE_ID { get; }
        public DbaseNumber TYPE { get; }
        public DbaseCharacter BESCHRIJV { get; }
        public DbaseCharacter OPERATOR { get; }
        public DbaseCharacter ORG { get; }
        public DbaseCharacter APPLICATIE { get; }
    }
}
