namespace RoadRegistry.BackOffice.Schema.RoadSegmentNationalRoadAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentNationalRoadAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new RoadSegmentNationalRoadAttributeDbaseSchema();

        public RoadSegmentNationalRoadAttributeDbaseRecord()
        {
            NW_OIDN = new DbaseNumber(Schema.NW_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            IDENT2 = new DbaseCharacter(Schema.IDENT2);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                NW_OIDN,
                WS_OIDN,
                IDENT2,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseNumber NW_OIDN { get; }
        public DbaseNumber WS_OIDN { get; }
        public DbaseCharacter IDENT2 { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
    }
}
