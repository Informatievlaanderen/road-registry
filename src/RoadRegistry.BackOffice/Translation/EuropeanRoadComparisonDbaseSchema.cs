namespace RoadRegistry.BackOffice.Translation
{
    using Aiv.Vbr.Shaperon;

    public class EuropeanRoadComparisonDbaseSchema : DbaseSchema
    {
        public EuropeanRoadComparisonDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(EU_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(EUNUMMER)),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName("transactID"),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName("recordtype"),
                        new DbaseFieldLength(4))
            };
        }

        public DbaseField EU_OIDN { get; }

        public DbaseField WS_OIDN { get; }

        public DbaseField EUNUMMER { get; }

        public DbaseField TransactID { get; }

        public DbaseField RecordType { get; }
    }
}
