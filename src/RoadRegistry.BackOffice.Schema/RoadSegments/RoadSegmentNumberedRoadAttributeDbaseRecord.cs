namespace RoadRegistry.BackOffice.Schema.RoadSegmentNumberedRoadAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentNumberedRoadAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentNumberedRoadAttributeDbaseSchema Schema = new RoadSegmentNumberedRoadAttributeDbaseSchema();

        public RoadSegmentNumberedRoadAttributeDbaseRecord()
        {
            GW_OIDN = new DbaseNumber(Schema.GW_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            IDENT8 = new DbaseCharacter(Schema.IDENT8);
            RICHTING = new DbaseNumber(Schema.RICHTING);
            LBLRICHT = new DbaseCharacter(Schema.LBLRICHT);
            VOLGNUMMER = new DbaseNumber(Schema.VOLGNUMMER);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                GW_OIDN,
                WS_OIDN,
                IDENT8,
                RICHTING,
                LBLRICHT,
                VOLGNUMMER,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseNumber GW_OIDN { get; }
        public DbaseNumber WS_OIDN { get; }
        public DbaseCharacter IDENT8 { get; }
        public DbaseNumber RICHTING { get; }
        public DbaseCharacter LBLRICHT { get; }
        public DbaseNumber VOLGNUMMER { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
    }
}
