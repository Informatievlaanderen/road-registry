namespace RoadRegistry.Projections
{
    using Shaperon;

    public class GradeSeparatedJunctionDbaseSchema
    {
        public GradeSeparatedJunctionDbaseSchema()
        {
            OK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(OK_OIDN)),
                new DbaseFieldLength(15));

            TYPE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(TYPE)),
                new DbaseFieldLength(2));

            LBLTYPE = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLTYPE)),
                new DbaseFieldLength(64));

            BO_WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(BO_WS_OIDN)),
                new DbaseFieldLength(15));

            ON_WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(ON_WS_OIDN)),
                new DbaseFieldLength(15));

            BEGINTIJD = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)));

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                new DbaseFieldLength(64));
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
