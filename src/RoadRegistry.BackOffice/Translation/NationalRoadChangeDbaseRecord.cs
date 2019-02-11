namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NationalRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly NationalRoadChangeDbaseSchema Schema = new NationalRoadChangeDbaseSchema();

        public NationalRoadChangeDbaseRecord()
        {
            NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            IDENT2 = new DbaseString(Schema.IDENT2);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                NW_OIDN,
                WS_OIDN,
                IDENT2,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 NW_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseString IDENT2 { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}
