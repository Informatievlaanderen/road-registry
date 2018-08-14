namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentNumberedRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentNumberedRoadAttributeDbaseSchema()
        {
            GW_OIDN = DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(GW_OIDN)),
                    new DbaseFieldLength(15));

            WS_OIDN = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15))
                .After(GW_OIDN);

            IDENT8 = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(IDENT8)),
                    new DbaseFieldLength(8))
                .After(WS_OIDN);

            RICHTING = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(RICHTING)),
                    new DbaseFieldLength(2))
                .After(IDENT8);

            LBLRICHT = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLRICHT)),
                    new DbaseFieldLength(64))
                .After(RICHTING);

            VOLGNUMMER = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(VOLGNUMMER)),
                    new DbaseFieldLength(5))
                .After(LBLRICHT);

            BEGINTIJD = DbaseField
                .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)))
                .After(VOLGNUMMER);

            BEGINORG = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18))
                .After(BEGINTIJD);

            LBLBGNORG = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLBGNORG)),
                    new DbaseFieldLength(64))
                .After(BEGINORG);

            Fields = new DbaseField[]
            {
                GW_OIDN, WS_OIDN, IDENT8, RICHTING, LBLRICHT, VOLGNUMMER, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField GW_OIDN { get; }
        public DbaseField WS_OIDN { get; }
        public DbaseField IDENT8 { get; }
        public DbaseField RICHTING { get; }
        public DbaseField LBLRICHT { get; }
        public DbaseField VOLGNUMMER { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
