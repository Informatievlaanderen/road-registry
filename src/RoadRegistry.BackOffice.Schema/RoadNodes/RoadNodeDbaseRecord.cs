namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeDbaseRecord : DbaseRecord
    {
        public static readonly RoadNodeDbaseSchema Schema = new RoadNodeDbaseSchema();

        public RoadNodeDbaseRecord()
        {
            WK_OIDN = new DbaseNumber(Schema.WK_OIDN);
            WK_UIDN = new DbaseCharacter(Schema.WK_UIDN);
            TYPE = new DbaseNumber(Schema.TYPE);
            LBLTYPE = new DbaseCharacter(Schema.LBLTYPE);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                WK_OIDN,WK_UIDN,TYPE,LBLTYPE,BEGINTIJD,BEGINORG,LBLBGNORG
            };
        }

        public DbaseNumber WK_OIDN { get; }
        public DbaseCharacter WK_UIDN { get; }
        public DbaseNumber TYPE { get; }
        public DbaseCharacter LBLTYPE { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
    }
}
