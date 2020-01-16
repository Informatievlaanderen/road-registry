namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadSegmentDirectionDbaseSchema : DbaseSchema
    {
        public NumberedRoadSegmentDirectionDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(RICHTING)),
                    new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLRICHT)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFRICHT)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField RICHTING => Fields[0];
        public DbaseField LBLRICHT => Fields[1];
        public DbaseField DEFRICHT => Fields[2];
    }
}
