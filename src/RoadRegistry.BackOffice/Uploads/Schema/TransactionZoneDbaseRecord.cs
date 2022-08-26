namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    namespace V1
    {
        public class TransactionZoneDbaseRecord : DbaseRecord
        {
            public static readonly TransactionZoneDbaseSchema Schema = new TransactionZoneDbaseSchema();

            public TransactionZoneDbaseRecord()
            {
                SOURCEID = new DbaseInt32(Schema.SOURCEID);
                TYPE = new DbaseInt32(Schema.TYPE);
                BESCHRIJV = new DbaseString(Schema.BESCHRIJV);
                OPERATOR = new DbaseString(Schema.OPERATOR);
                ORG = new DbaseString(Schema.ORG);
                APPLICATIE = new DbaseString(Schema.APPLICATIE);

                Values = new DbaseFieldValue[]
                {
                    SOURCEID, TYPE, BESCHRIJV, OPERATOR, ORG, APPLICATIE
                };
            }

            public DbaseInt32 SOURCEID { get; }
            public DbaseInt32 TYPE { get; }
            public DbaseString BESCHRIJV { get; }
            public DbaseString OPERATOR { get; }
            public DbaseString ORG { get; }
            public DbaseString APPLICATIE { get; }
        }
    }

    namespace V2
    {
        public class TransactionZoneDbaseRecord : DbaseRecord
        {
            public static readonly TransactionZoneDbaseSchema Schema = new TransactionZoneDbaseSchema();

            public TransactionZoneDbaseRecord()
            {
                SOURCEID = new DbaseInt32(Schema.SOURCEID);
                TYPE = new DbaseInt32(Schema.TYPE);
                BESCHRIJV = new DbaseString(Schema.BESCHRIJV);
                OPERATOR = new DbaseString(Schema.OPERATOR);
                ORG = new DbaseString(Schema.ORG);
                APPLICATIE = new DbaseString(Schema.APPLICATIE);
                DOWNLOADID = new DbaseString(Schema.DOWNLOADID);

                Values = new DbaseFieldValue[]
                {
                SOURCEID, TYPE, BESCHRIJV, OPERATOR, ORG, APPLICATIE, DOWNLOADID
                };
            }

            public DbaseInt32 SOURCEID { get; }
            public DbaseInt32 TYPE { get; }
            public DbaseString BESCHRIJV { get; }
            public DbaseString OPERATOR { get; }
            public DbaseString ORG { get; }
            public DbaseString APPLICATIE { get; }
            public DbaseString DOWNLOADID { get; }
        }
    }
}
