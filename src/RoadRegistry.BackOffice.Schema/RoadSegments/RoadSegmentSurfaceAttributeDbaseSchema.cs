namespace RoadRegistry.BackOffice.Schema.RoadSegmentSurfaceAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentSurfaceAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentSurfaceAttributeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WV_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(WS_GIDN)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(VANPOS)),
                        new DbaseFieldLength(9),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(TOTPOS)),
                        new DbaseFieldLength(9),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD))),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(BEGINORG)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLBGNORG)),
                        new DbaseFieldLength(64))
            };
        }

        public DbaseField WV_OIDN => Fields[0];
        public DbaseField WS_OIDN => Fields[1];
        public DbaseField WS_GIDN => Fields[2];
        public DbaseField TYPE => Fields[3];
        public DbaseField LBLTYPE => Fields[4];
        public DbaseField VANPOS => Fields[5];
        public DbaseField TOTPOS => Fields[6];
        public DbaseField BEGINTIJD => Fields[7];
        public DbaseField BEGINORG => Fields[8];
        public DbaseField LBLBGNORG => Fields[9];
    }
}
