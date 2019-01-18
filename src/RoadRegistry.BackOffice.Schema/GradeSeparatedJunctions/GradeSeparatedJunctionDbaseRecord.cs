namespace RoadRegistry.BackOffice.Schema.GradeSeparatedJunctions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionDbaseRecord : DbaseRecord
    {
        public static readonly GradeSeparatedJunctionDbaseSchema Schema = new GradeSeparatedJunctionDbaseSchema();

        public GradeSeparatedJunctionDbaseRecord()
        {
            OK_OIDN = new DbaseInt32(Schema.OK_OIDN);
            TYPE = new DbaseInt32(Schema.TYPE);
            LBLTYPE = new DbaseString(Schema.LBLTYPE);
            BO_WS_OIDN = new DbaseInt32(Schema.BO_WS_OIDN);
            ON_WS_OIDN = new DbaseInt32(Schema.ON_WS_OIDN);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseString(Schema.BEGINORG);
            LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                OK_OIDN,
                TYPE,
                LBLTYPE,
                BO_WS_OIDN,
                ON_WS_OIDN,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseInt32 OK_OIDN { get; }
        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseInt32 BO_WS_OIDN { get; }
        public DbaseInt32 ON_WS_OIDN { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
    }
}
