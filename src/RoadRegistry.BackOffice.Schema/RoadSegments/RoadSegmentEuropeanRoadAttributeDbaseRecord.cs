namespace RoadRegistry.BackOffice.Schema.RoadSegmentEuropeanRoadAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentEuropeanRoadAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new RoadSegmentEuropeanRoadAttributeDbaseSchema();

        public RoadSegmentEuropeanRoadAttributeDbaseRecord()
        {
            EU_OIDN = new DbaseNumber(Schema.EU_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            EUNUMMER = new DbaseCharacter(Schema.EUNUMMER);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                EU_OIDN,
                WS_OIDN,
                EUNUMMER,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseNumber EU_OIDN { get; }
        public DbaseNumber WS_OIDN { get; }
        public DbaseCharacter EUNUMMER { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
    }
}
