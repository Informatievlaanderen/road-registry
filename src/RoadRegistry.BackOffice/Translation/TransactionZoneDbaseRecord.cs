namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class TransactionZoneDbaseRecord : DbaseRecord
    {
        public static readonly TransactionZoneDbaseSchema Schema = new TransactionZoneDbaseSchema();

        public TransactionZoneDbaseRecord()
        {
            SOURCE_ID = new DbaseInt32(Schema.SOURCE_ID);
            TYPE = new DbaseInt32(Schema.TYPE);
            BESCHRIJV = new DbaseString(Schema.BESCHRIJV);
            OPERATOR = new DbaseString(Schema.OPERATOR);
            ORG = new DbaseString(Schema.ORG);
            APPLICATIE = new DbaseString(Schema.APPLICATIE);

            Values = new DbaseFieldValue[]
            {
                SOURCE_ID, TYPE, BESCHRIJV, OPERATOR, ORG, APPLICATIE
            };
        }

        public DbaseInt32 SOURCE_ID { get; }
        public DbaseInt32 TYPE { get; }
        public DbaseString BESCHRIJV { get; }
        public DbaseString OPERATOR { get; }
        public DbaseString ORG { get; }
        public DbaseString APPLICATIE { get; }
    }
}
