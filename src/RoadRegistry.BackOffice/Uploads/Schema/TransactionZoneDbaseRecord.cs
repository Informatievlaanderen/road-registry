namespace RoadRegistry.BackOffice.Uploads.Schema
{
    namespace V1
    {
        using Be.Vlaanderen.Basisregisters.Shaperon;

        public class TransactionZoneDbaseRecord : DbaseRecord
        {
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

            public DbaseString APPLICATIE { get; }
            public DbaseString BESCHRIJV { get; }
            public DbaseString OPERATOR { get; }
            public DbaseString ORG { get; }
            public static readonly TransactionZoneDbaseSchema Schema = new();

            public DbaseInt32 SOURCEID { get; }
            public DbaseInt32 TYPE { get; }
        }
    }

    namespace V2
    {
        using Be.Vlaanderen.Basisregisters.Shaperon;

        public class TransactionZoneDbaseRecord : DbaseRecord
        {
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

            public DbaseString APPLICATIE { get; }
            public DbaseString BESCHRIJV { get; }
            public DbaseString DOWNLOADID { get; }
            public DbaseString OPERATOR { get; }
            public DbaseString ORG { get; }
            public static readonly TransactionZoneDbaseSchema Schema = new();

            public DbaseInt32 SOURCEID { get; }
            public DbaseInt32 TYPE { get; }
        }
    }
}