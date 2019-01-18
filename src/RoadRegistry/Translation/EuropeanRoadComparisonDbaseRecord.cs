namespace RoadRegistry.Translation
{
    using System;
    using System.Collections.Generic;
    using Aiv.Vbr.Shaperon;

    public class EuropeanRoadComparisonDbaseRecord : DbaseRecord
    {
        public static readonly EuropeanRoadComparisonDbaseSchema Schema = new EuropeanRoadComparisonDbaseSchema();

        public EuropeanRoadComparisonDbaseRecord()
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

        public void PopulateFrom(DbaseRecord record)
        {
            var index = new Dictionary<DbaseField, Action<DbaseFieldValue>>
            {
                {EU_OIDN.Field, value => value.CopyValueTo(EU_OIDN)},
                {WS_OIDN.Field, value => value.CopyValueTo(WS_OIDN)},
                {EUNUMMER.Field, value => value.CopyValueTo(EUNUMMER)},
                {TransactID.Field, value => value.CopyValueTo(TransactID)},
                {RecordType.Field, value => value.CopyValueTo(RecordType)}
            };
            foreach (var value in record.Values)
            {
                if (index.TryGetValue(value.Field, out var copier))
                {
                    copier(value);
                }
            }
        }
    }
}
