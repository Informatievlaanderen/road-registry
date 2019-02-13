namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionChangeDbaseRecord : DbaseRecord
    {
        public static readonly GradeSeparatedJunctionChangeDbaseSchema Schema = new GradeSeparatedJunctionChangeDbaseSchema();

        public GradeSeparatedJunctionChangeDbaseRecord()
        {
            OK_OIDN = new DbaseInt32(Schema.OK_OIDN);
            TYPE = new DbaseInt16(Schema.TYPE);
            BO_WS_OIDN = new DbaseInt32(Schema.BO_WS_OIDN);
            ON_WS_OIDN = new DbaseInt32(Schema.ON_WS_OIDN);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                OK_OIDN,
                TYPE,
                BO_WS_OIDN,
                ON_WS_OIDN,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 OK_OIDN { get; }

        public DbaseInt16 TYPE { get; }

        public DbaseInt32 BO_WS_OIDN { get; }

        public DbaseInt32 ON_WS_OIDN { get; }

        public DbaseDouble VAN_POSITIE { get; }

        public DbaseDouble TOT_POSITIE { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}
