namespace RoadRegistry.Projections
{
    using Shaperon;

    public class GradeSeparatedJunctionDbaseSchema : DbaseSchema
    {
        public GradeSeparatedJunctionDbaseSchema()
        {
            OK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(OK_OIDN)),
                new DbaseFieldLength(15));

            TYPE = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2))
                .After(OK_OIDN);

            LBLTYPE = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64))
                .After(TYPE);

            BO_WS_OIDN = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(BO_WS_OIDN)),
                    new DbaseFieldLength(15))
                .After(LBLTYPE);

            ON_WS_OIDN = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(ON_WS_OIDN)),
                    new DbaseFieldLength(15))
                .After(BO_WS_OIDN);

            BEGINTIJD = DbaseField
                .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)))
                .After(ON_WS_OIDN);

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
                OK_OIDN, TYPE, LBLTYPE, BO_WS_OIDN, ON_WS_OIDN, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField OK_OIDN { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BO_WS_OIDN { get; }
        public DbaseField ON_WS_OIDN { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
