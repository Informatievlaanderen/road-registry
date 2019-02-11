namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly NumberedRoadChangeDbaseSchema Schema = new NumberedRoadChangeDbaseSchema();

        public NumberedRoadChangeDbaseRecord()
        {
            GW_OIDN = new DbaseInt32(Schema.GW_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            IDENT8 = new DbaseString(Schema.IDENT8);
            RICHTING = new DbaseInt16(Schema.RICHTING);
            VOLGNUMMER = new DbaseInt32(Schema.VOLGNUMMER);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                GW_OIDN,
                WS_OIDN,
                IDENT8,
                RICHTING,
                VOLGNUMMER,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 GW_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseString IDENT8 { get; }

        public DbaseInt16 RICHTING { get; }

        public DbaseInt32 VOLGNUMMER { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}
