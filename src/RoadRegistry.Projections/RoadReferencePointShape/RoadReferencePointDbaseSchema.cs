namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadReferencePointDbaseSchema : DbaseSchema
    {
        public RoadReferencePointDbaseSchema()
        {
            RP_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RP_OIDN)),
                new DbaseFieldLength(15));

            RP_UIDN = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(RP_UIDN)),
                    new DbaseFieldLength(18))
                .After(RP_OIDN);

            IDENT8 =  DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(IDENT8)),
                    new DbaseFieldLength(8))
                .After(RP_UIDN);

            OPSCHRIFT = DbaseField
                .CreateDoubleField(
                    new DbaseFieldName(nameof(OPSCHRIFT)),
                    new DbaseFieldLength(5),
                    new DbaseDecimalCount(1))
                .After(IDENT8);

            TYPE = DbaseField
                .CreateInt32Field(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2))
                .After(OPSCHRIFT);

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
                RP_UIDN, RP_OIDN, IDENT8, OPSCHRIFT, TYPE, LBLTYPE, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField RP_UIDN { get; }
        public DbaseField RP_OIDN { get; }
        public DbaseField IDENT8 { get; }
        public DbaseField OPSCHRIFT { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
