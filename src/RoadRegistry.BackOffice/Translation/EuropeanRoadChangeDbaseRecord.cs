namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class EuropeanRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly EuropeanRoadChangeDbaseSchema Schema = new EuropeanRoadChangeDbaseSchema();

        public EuropeanRoadChangeDbaseRecord()
        {
            EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            EUNUMMER = new DbaseString(Schema.EUNUMMER);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                EU_OIDN,
                WS_OIDN,
                EUNUMMER,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 EU_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseString EUNUMMER { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}
