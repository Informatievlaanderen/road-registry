namespace RoadRegistry.BackOffice.Schema.GradeSeparatedJunctions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionDbaseRecord : DbaseRecord
    {
        public static readonly GradeSeparatedJunctionDbaseSchema Schema = new GradeSeparatedJunctionDbaseSchema();

        public GradeSeparatedJunctionDbaseRecord()
        {
            OK_OIDN = new DbaseNumber(Schema.OK_OIDN);
            TYPE = new DbaseNumber(Schema.TYPE);
            LBLTYPE = new DbaseCharacter(Schema.LBLTYPE);
            BO_WS_OIDN = new DbaseNumber(Schema.BO_WS_OIDN);
            ON_WS_OIDN = new DbaseNumber(Schema.ON_WS_OIDN);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

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

        public DbaseNumber OK_OIDN { get; }
        public DbaseNumber TYPE { get; }
        public DbaseCharacter LBLTYPE { get; }
        public DbaseNumber BO_WS_OIDN { get; }
        public DbaseNumber ON_WS_OIDN { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
    }
}
