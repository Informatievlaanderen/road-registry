namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadNodeDbaseSchema : DbaseSchema
    {
        public RoadNodeDbaseSchema()
        {
            WK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WK_OIDN)),
                new DbaseFieldLength(15));

            WK_UIDN = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(WK_UIDN)),
                    new DbaseFieldLength(18))
                .After(WK_OIDN);

            TYPE = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2))
                .After(WK_UIDN);

            LBLTYPE = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64))
                .After(TYPE);

            BEGINTIJD = DbaseField
                .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)))
                .After(LBLTYPE);

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
                WK_OIDN, WK_UIDN, TYPE, LBLTYPE, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField WK_OIDN { get; }
        public DbaseField WK_UIDN { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
