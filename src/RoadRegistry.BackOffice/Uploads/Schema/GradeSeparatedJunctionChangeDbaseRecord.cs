namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionChangeDbaseRecord : DbaseRecord
    {
        public static readonly GradeSeparatedJunctionChangeDbaseSchema Schema = new GradeSeparatedJunctionChangeDbaseSchema();

        public GradeSeparatedJunctionChangeDbaseRecord()
        {
            OK_OIDN = new DbaseNumber(Schema.OK_OIDN);
            TYPE = new DbaseNumber(Schema.TYPE);
            BO_WS_OIDN = new DbaseNumber(Schema.BO_WS_OIDN);
            ON_WS_OIDN = new DbaseNumber(Schema.ON_WS_OIDN);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                OK_OIDN,
                TYPE,
                BO_WS_OIDN,
                ON_WS_OIDN,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber OK_OIDN { get; }

        public DbaseNumber TYPE { get; }

        public DbaseNumber BO_WS_OIDN { get; }

        public DbaseNumber ON_WS_OIDN { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
